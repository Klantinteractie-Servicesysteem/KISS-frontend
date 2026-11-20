namespace Kiss.Bff.EndToEndTest
{
    public class AzureAdLoginHelper
    {
        private readonly IPage _page;
        private readonly string _username;
        private readonly string _password;
        private readonly UniqueOtpHelper _uniqueOtpHelper;

        public AzureAdLoginHelper(IPage page, string username, string password, UniqueOtpHelper uniqueOtpHelper)
        {
            _page = page;
            _username = username;
            _password = password;
            _uniqueOtpHelper = uniqueOtpHelper;
        }

        public async Task LoginAsync()
        {
            // the caller is expected to have already navigated to the login page
            // and selected the ICATT identity provider (see KissPlaywrightTest.EnsureIcattSelectedAsync)
            var usernameInput = _page.Locator("input[name='loginfmt']");

            // Microsoft sometimes remembers the account (SSO) and skips straight to the
            // 2FA "Enter code" step, bypassing the username/password forms entirely.
            var otpAlreadyShowing = _page.GetByRole(AriaRole.Textbox, new() { Name = "Code" })
                .Or(_page.Locator("input[name='otc']"))
                .Or(_page.Locator("#idTxtBx_SAOTCC_OTC"))
                .Or(_page.GetByRole(AriaRole.Link, new() { Name = "Microsoft Authenticator" }));

            // The identity provider may have a valid, non-expired SSO session (e.g. from a previous
            // test run) and silently redirect straight back to the already-logged-in app, bypassing
            // the username/password/2FA forms entirely. Detect this outcome too, otherwise we'd wait
            // forever for login fields that will never appear.
            var alreadyLoggedInIndicator = _page.GetByRole(AriaRole.Link, new() { Name = "Uitloggen" })
                .Or(_page.GetByRole(AriaRole.Button, new() { Name = "Nieuw contactmoment" }));

            try
            {
                await usernameInput.Or(otpAlreadyShowing.First).Or(alreadyLoggedInIndicator.First).First.WaitForAsync();
            }
            catch (TimeoutException)
            {
                await CaptureDiagnosticsAsync("login-step-timeout");
                throw;
            }

            if (await alreadyLoggedInIndicator.First.IsVisibleAsync())
            {
                // silent SSO already completed; nothing left to do
                return;
            }

            if (await otpAlreadyShowing.First.IsVisibleAsync())
            {
                // already past username/password, go straight to 2FA
                await Handle2FAAsync();
                return;
            }

            await usernameInput.FillAsync(_username);
            await _page.ClickAsync("input[type='submit']");

            await _page.WaitForSelectorAsync("input[name='passwd']");
            await _page.FillAsync("input[name='passwd']", _password);
            await _page.ClickAsync("input[type='submit']");

            await Handle2FAAsync();
        }

        private async Task Handle2FAAsync()
        {
            var enterManuallyLink = _page.GetByRole(AriaRole.Link, new() { Name = "Microsoft Authenticator" });

            var verifyButton = _page.GetByRole(AriaRole.Button, new() { Name = "Verifiëren" })
                .Or(_page.GetByRole(AriaRole.Button, new() { Name = "Verify" }))
                .Or(_page.Locator("#idSubmit_SAOTCC_Continue"));

            var declineStaySignedInButton = _page.GetByRole(AriaRole.Button, new() { Name = "Nee" })
                .Or(_page.GetByRole(AriaRole.Button, new() { Name = "No" }));

            var nieuwContactmomentSelector = _page.GetByRole(AriaRole.Button, new() { Name = "Nieuw contactmoment" });

            // Microsoft's OTP input doesn't always have a reliable accessible name across locales/tenants,
            // so fall back to the well-known input name/id used by the identity platform's code entry page.
            var totpBoxSelector = _page.GetByRole(AriaRole.Textbox, new() { Name = "Code" })
                .Or(_page.Locator("input[name='otc']"))
                .Or(_page.Locator("#idTxtBx_SAOTCC_OTC"));



            // we will either get the 2FA code input and the submit button,
            // or an option to enter the code manually
            await totpBoxSelector.Or(enterManuallyLink).First.WaitForAsync();

            if (!await totpBoxSelector.First.IsVisibleAsync() && await enterManuallyLink.IsVisibleAsync())
            {
                await enterManuallyLink.ClickAsync();
                await totpBoxSelector.First.WaitForAsync();
            }

            // Generate TOTP code
            var totpCode = await _uniqueOtpHelper.GetUniqueCode();
            // Fill in the TOTP code
            await totpBoxSelector.First.FillAsync(totpCode);
            await verifyButton.First.ClickAsync();

            // we will now either go back to the home page, or get an option to stay signed in
            await declineStaySignedInButton
                .Or(nieuwContactmomentSelector)
                .WaitForAsync();

            if (await declineStaySignedInButton.IsVisibleAsync())
            {
                await declineStaySignedInButton.ClickAsync();
            }
        }

        /// <summary>
        /// Captures a screenshot and the current URL to help diagnose login flow failures
        /// (e.g. network/environment issues preventing the identity-provider redirect from completing).
        /// </summary>
        private async Task CaptureDiagnosticsAsync(string label)
        {
            try
            {
                var dir = Path.Combine(Environment.CurrentDirectory, "screenshots");
                Directory.CreateDirectory(dir);
                var path = Path.Combine(dir, $"{label}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.png");
                await _page.ScreenshotAsync(new() { Path = path, FullPage = true });

                var bodyText = await _page.Locator("body").InnerTextAsync();
                var truncated = bodyText.Length > 2000 ? bodyText.Substring(0, 2000) + "... (truncated)" : bodyText;

                Console.WriteLine($"[AzureAdLoginHelper] Diagnostic screenshot saved to {path}. Current URL: {_page.Url}");
                Console.WriteLine($"[AzureAdLoginHelper] Page body text:\n{truncated}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AzureAdLoginHelper] Failed to capture diagnostics: {ex.Message}. Current URL: {_page.Url}");
            }
        }
    }
}

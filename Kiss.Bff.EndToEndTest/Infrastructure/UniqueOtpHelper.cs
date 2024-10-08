﻿using OtpNet;

namespace Kiss.Bff.EndToEndTest
{
    /// <summary>
    /// The Azure AD 2FA login method checks to see the same one-time-password is only used once.
    /// Because of this, we need to make sure the same OTP is not re-used across tests.
    /// We cannot change the lifetime of OTPs, so we actually need to wait until the previous one expires.
    /// </summary>
    public class UniqueOtpHelper
    {
        private readonly Totp _otp;
        private string? _oldCode;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public UniqueOtpHelper(string totpSecret)
        {
            _otp = new Totp(Base32Encoding.ToBytes(totpSecret));
        }

        public async Task<string> GetUniqueCode()
        {
            await _semaphore.WaitAsync();
            try
            {
                var remainingSeconds = _otp.RemainingSeconds();
                var newCode = _otp.ComputeTotp();
                if (newCode == _oldCode)
                {
                    await Task.Delay(TimeSpan.FromSeconds(remainingSeconds));
                    newCode = _otp.ComputeTotp();
                }
                _oldCode = newCode;
                return newCode;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}

import DOMPurify from "dompurify";

DOMPurify.addHook("afterSanitizeAttributes", (node) => {
    if (node instanceof HTMLAnchorElement && !node.href.startsWith("#")) {
        node.target = "_blank";
        node.relList.add("noopener", "noreferrer");
    }
});

type HeadingLevel = 1 | 2 | 3 | 4 | 5 | 6;
const headerRegex = /(<\/?h)([1-6])(>)/g;

export function increaseHeadings(html: string, level: HeadingLevel): string {
    return html.replace(headerRegex, (_, open, l, close) => {
        const newLevel = Number.parseInt(l, 10) + level - 1;
        const classes = open.includes("/")
            ? ""
            : ` class="utrecht-heading-${newLevel}"`;
        return `${open}${newLevel}${classes}${close}`;
    });
}

export function sanitizeHtmlToBerichtFormat(str: string) {
    return DOMPurify.sanitize(str, {
        FORBID_ATTR: ["style"],
        FORBID_TAGS: ["img"],
    });
}

const escapedChars = {
    "&quot;": '"',
    "&#34;": '"',

    "&apos;": "'",
    "&#39;": "'",

    "&amp;": "&",
    "&#38;": "&",

    "&gt;": ">",
    "&#62;": ">",

    "&lt;": "<",
    "&#60;": "<",
} as const;

const escapedSet = new Map<string, string>(Object.entries(escapedChars));

function toRegex(chars: Record<string, string>) {
    const keys = Object.keys(chars).join("|");
    const regex = new RegExp("(?=(" + keys + "))\\1", "g");
    return regex;
}

const escapeRegex = toRegex(escapedChars);

export function unescapeHtml(str: string): string {
    return str.replace(escapeRegex, (m) => escapedSet.get(m) || m);
}

export function focusNextFormItem(
    element: Element & { form?: HTMLFormElement | null }
) {
    setTimeout(() => {
        if (!element.form) return;
        const elements = Array.from(
            element.form.querySelectorAll(
                'button:not([disabled]), [href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"]):not([disabled]), details:not([disabled]), summary:not(:disabled)'
            )
        );
        const i = elements.indexOf(element);
        if (i === -1 || i >= elements.length) return;
        const next = elements[i + 1];
        if (next instanceof HTMLElement) {
            next.focus();
        }
    });
}


export function unescapedSanatizedWithIncreadesHeadingsHtml(html: string, headingLevel: number) {
    if (!html) {
        return html;
    }
    const unescapedHtml = unescapeHtml(html);
    const cleanedHtml = sanitizeHtmlToBerichtFormat(unescapedHtml);
    const htmlWithIncreasedHeadings = increaseHeadings(
        cleanedHtml,
        (headingLevel + 1) as any
    );
    return htmlWithIncreasedHeadings;
}
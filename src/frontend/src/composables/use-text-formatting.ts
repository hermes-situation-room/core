/**
 * Simple markdown-style text formatting utility
 * Supports: **bold**, *italic*, bullet lists (- or *), and [color=...]text[/color]
 */

/**
 * Allowlist of safe named colors that can be used in [color=...] markup.
 * Anything outside this set (and outside the hex pattern below) is stripped,
 * so no arbitrary CSS/markup can be injected through the color syntax.
 */
const ALLOWED_COLOR_NAMES = [
    'red',
    'orange',
    'yellow',
    'green',
    'teal',
    'blue',
    'purple',
    'pink',
    'gray',
    'grey',
    'black',
    'white',
] as const;

const HEX_COLOR_PATTERN = /^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6})$/;

/**
 * Validates a color token against the allowlist.
 * Returns the safe, normalized color string or null if it is not allowed.
 */
const sanitizeColor = (rawColor: string): string | null => {
    const color = rawColor.trim().toLowerCase();
    if (ALLOWED_COLOR_NAMES.includes(color as (typeof ALLOWED_COLOR_NAMES)[number])) {
        return color;
    }
    if (HEX_COLOR_PATTERN.test(color)) {
        return color;
    }
    return null;
};

export function useTextFormatting() {
    const formatText = (text: string): string => {
        if (!text) return '';

        let formatted = text;

        // Escape HTML to prevent XSS
        formatted = escapeHtml(formatted);

        // Convert [color=...]...[/color] to a <span> with an allowlisted color.
        // Runs after escaping so the captured text is already safe; the color
        // token is validated against the allowlist and unsafe colors are stripped
        // (the inner text is preserved without any color span).
        formatted = formatColors(formatted);

        // Convert **bold** to <strong>
        formatted = formatted.replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>');

        // Convert *italic* to <em> (but not if it's part of **)
        formatted = formatted.replace(/(?<!\*)\*(?!\*)(.+?)\*(?!\*)/g, '<em>$1</em>');

        // Convert bullet lists (lines starting with - or * followed by space)
        formatted = formatLists(formatted);

        // Convert newlines to <br> for regular lines
        formatted = formatted.replace(/\n/g, '<br>');

        return formatted;
    };

    /**
     * Translates [color=<name|#hex>]text[/color] into a colored <span>.
     * Only allowlisted color names and short/long hex values are honored;
     * any other color value is dropped and the inner text is kept uncolored.
     */
    const formatColors = (text: string): string => {
        // The color token is restricted to word chars and '#' so the regex itself
        // cannot capture markup; sanitizeColor then enforces the actual allowlist.
        return text.replace(/\[color=([#\w]+)\]([\s\S]*?)\[\/color\]/g, (_match, colorToken: string, inner: string) => {
            const safeColor = sanitizeColor(colorToken);
            if (!safeColor) {
                return inner;
            }
            return `<span style="color: ${safeColor}">${inner}</span>`;
        });
    };

    const formatLists = (text: string): string => {
        const lines = text.split('\n');
        let inList = false;
        const result: string[] = [];

        for (let i = 0; i < lines.length; i++) {
            const line = lines[i] ?? '';
            const listMatch = line.match(/^[\s]*[-*]\s+(.+)$/);

            if (listMatch) {
                if (!inList) {
                    result.push('<ul>');
                    inList = true;
                }
                result.push(`<li>${listMatch[1]}</li>`);
            } else {
                if (inList) {
                    result.push('</ul>');
                    inList = false;
                }
                result.push(line);
            }
        }

        if (inList) {
            result.push('</ul>');
        }

        return result.join('\n');
    };

    const escapeHtml = (text: string): string => {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    };

    const getFormattingHelp = (): string[] => {
        return [
            '**bold text** for bold',
            '*italic text* for italic',
            '- item or * item for bullet lists',
            '[color=red]text[/color] for colored text',
        ];
    };

    const getAllowedColors = (): string[] => {
        return [...ALLOWED_COLOR_NAMES];
    };

    return {
        formatText,
        getFormattingHelp,
        getAllowedColors,
    };
}

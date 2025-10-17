/**
 * Simple markdown-style text formatting utility
 * Supports: **bold**, *italic*, and bullet lists (- or *)
 */

export function useTextFormatting() {
    const formatText = (text: string): string => {
        if (!text) return '';
        
        let formatted = text;
        
        // Escape HTML to prevent XSS
        formatted = escapeHtml(formatted);
        
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
    
    const formatLists = (text: string): string => {
        const lines = text.split('\n');
        let inList = false;
        const result: string[] = [];
        
        for (let i = 0; i < lines.length; i++) {
            const line = lines[i];
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
        ];
    };
    
    return {
        formatText,
        getFormattingHelp,
    };
}


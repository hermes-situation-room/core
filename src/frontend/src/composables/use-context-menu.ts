import { ref } from 'vue';

export function useContextMenu() {
    const contextMenuItemId = ref<string | null>(null);
    const contextMenuPosition = ref({ x: 0, y: 0 });
    const showContextMenu = ref(false);
    const showMobileMenu = ref<string | null>(null);

    const handleRightClick = (event: MouseEvent, itemId: string) => {
        event.preventDefault();
        event.stopPropagation();
        
        showMobileMenu.value = null;
        
        contextMenuItemId.value = itemId;
        contextMenuPosition.value = { x: event.clientX, y: event.clientY };
        showContextMenu.value = true;
        
        const closeMenu = () => {
            showContextMenu.value = false;
            document.removeEventListener('click', closeMenu);
        };
        setTimeout(() => document.addEventListener('click', closeMenu), 0);
    };

    const toggleMobileMenu = (event: Event, itemId: string) => {
        event.stopPropagation();
        
        showContextMenu.value = false;
        
        if (showMobileMenu.value === itemId) {
            showMobileMenu.value = null;
        } else {
            showMobileMenu.value = itemId;
            const closeMenu = (e: MouseEvent) => {
                const target = e.target as HTMLElement;
                if (!target.closest('.position-relative')) {
                    showMobileMenu.value = null;
                    document.removeEventListener('click', closeMenu);
                }
            };
            setTimeout(() => document.addEventListener('click', closeMenu), 0);
        }
    };

    const closeAllMenus = () => {
        showContextMenu.value = false;
        showMobileMenu.value = null;
    };

    return {
        contextMenuItemId,
        contextMenuPosition,
        showContextMenu,
        showMobileMenu,
        
        handleRightClick,
        toggleMobileMenu,
        closeAllMenus
    };
}



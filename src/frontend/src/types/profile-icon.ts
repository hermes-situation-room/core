export enum ProfileIcon {
    User = 'User',
    UserCircle = 'UserCircle',
    Star = 'Star',
    Heart = 'Heart',
    Rocket = 'Rocket',
    Shield = 'Shield',
    Zap = 'Zap',
    Camera = 'Camera',
    Smile = 'Smile',
    Globe = 'Globe',
    Music = 'Music',
    Briefcase = 'Briefcase',
    Book = 'Book',
    Coffee = 'Coffee',
    Mountain = 'Mountain',
    Flame = 'Flame',
    Crown = 'Crown',
    Sparkles = 'Sparkles'
}

export enum ProfileIconColor {
    Blue = 'Blue',
    LightBlue = 'LightBlue',
    Purple = 'Purple',
    Green = 'Green',
    Orange = 'Orange',
    Pink = 'Pink',
    Red = 'Red',
    Black = 'Black',
    Gray = 'Gray'
}

export const ICON_MAP = {
    User: 'fa-user',
    UserCircle: 'fa-user-circle',
    Star: 'fa-star',
    Heart: 'fa-heart',
    Rocket: 'fa-rocket',
    Shield: 'fa-shield-alt',
    Zap: 'fa-bolt',
    Camera: 'fa-camera',
    Smile: 'fa-smile',
    Globe: 'fa-globe',
    Music: 'fa-music',
    Briefcase: 'fa-briefcase',
    Book: 'fa-book',
    Coffee: 'fa-coffee',
    Mountain: 'fa-mountain',
    Flame: 'fa-fire',
    Crown: 'fa-crown',
    Sparkles: 'fa-sparkles'
} as const;

export const COLOR_MAP: Record<ProfileIconColor, string> = {
    [ProfileIconColor.Blue]: '#3b82f6',
    [ProfileIconColor.LightBlue]: '#22c5c0',
    [ProfileIconColor.Green]: '#22c55e',
    [ProfileIconColor.Orange]: '#f97316',
    [ProfileIconColor.Pink]: '#ec4899',
    [ProfileIconColor.Purple]: '#a855f7',
    [ProfileIconColor.Red]: '#dc2d2d',
    [ProfileIconColor.Black]: '#232323',
    [ProfileIconColor.Gray]: '#888888'
};

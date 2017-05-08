export class Card {
    id: number;
    word: string;
    transcription: string;
    translation: string;
    imageURL: string;
    soundURL: string;
    collectionId: number;

    constructor() {
        // set url's to empty string to avoid browser errors
        this.imageURL = '';
        this.soundURL = '';
    }
}

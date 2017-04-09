import { Component, Input } from '@angular/core';

@Component({
    selector: 'app-loading-container',
    // templateUrl: 'loading-container.component.html'
    template: '<md-spinner *ngIf="loading"></md-spinner>'
})
export class LoadingContainerComponent {
    @Input() loading: boolean;
    constructor() { }
}

export class LoadingPage {
    public loading: boolean;
    constructor(val: boolean) {
        this.loading = val;
    }
    standby() {
        this.loading = true;
    }
    ready() {
        this.loading = false;
    }
}

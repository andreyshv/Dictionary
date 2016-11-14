import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import { CardsComponent } from './cards.component';
import { CardDetailComponent } from './card-detail.component';

@NgModule({
    imports: [
        RouterModule.forChild([
            {
                path: 'cards',
                component: CardsComponent
            },
            {
                path: 'card-detail/:id',
                component: CardDetailComponent
            },
            {
                path: 'new-card',
                component: CardDetailComponent
            }
        ])
    ],
    exports: [
        RouterModule
    ],
    declarations: [],
    providers: [],
})
export class CardsRoutingModule { }

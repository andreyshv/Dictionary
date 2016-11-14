import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { InfiniteScrollModule } from 'angular2-infinite-scroll';
import { LoadingContainerComponent } from '../../ext/loading-container.component';

import { CardsComponent } from './cards.component';
import { CardDetailComponent } from './card-detail.component';
import { ImageSearchComponent } from './image-search.component';

import { CardService } from './card.service';
import { MediaSearchService } from './media-search.service';

import { CardsRoutingModule } from './cards-routing.module';

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        InfiniteScrollModule,
        CardsRoutingModule
    ],
    exports: [],
    declarations: [
        LoadingContainerComponent,
        CardsComponent,
        CardDetailComponent,
        ImageSearchComponent
    ],
    providers: [
        CardService,
        MediaSearchService
    ],
})
export class CardsModule { }

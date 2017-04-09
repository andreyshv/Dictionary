import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpModule } from '@angular/http';
import { FormsModule } from '@angular/forms';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { AppConfigService } from './app.config.service';
import { CollectionService } from './collection.service';

import { AppComponent } from './app.component';
import { CardsModule } from './cards/cards.module';

import { CollectionsComponent } from './collections.component';
import { CollectionDetailComponent } from './collection-detail.component';
import { QueueComponent } from './queue.component';
import { LoadingContainerComponent } from '../ext/loading-container.component';

import { DashboardComponent } from './dashboard.component';

import { routing } from './app.routing';

@NgModule({
    imports: [
        // angular
        BrowserModule,
        FormsModule,
        HttpModule,
        BrowserAnimationsModule,
        // app
        CardsModule,
        routing
    ],
    declarations: [
        // app
        AppComponent,
        CollectionsComponent,
        CollectionDetailComponent,
        QueueComponent,
        // other
        DashboardComponent
    ],
    providers: [
        CookieService,
        AppConfigService,
        CollectionService
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }

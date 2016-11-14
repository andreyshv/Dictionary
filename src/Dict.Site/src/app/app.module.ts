//import './rxjs-extensions';
// Observable class extensions
import 'rxjs/add/observable/of';
import 'rxjs/add/observable/throw';

// Observable operators
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/debounceTime';
import 'rxjs/add/operator/distinctUntilChanged';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/filter';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/switchMap';
import 'rxjs/add/operator/toPromise';

import { NgModule } from '@angular/core';
import { BrowserModule }  from '@angular/platform-browser';
import { HttpModule } from '@angular/http';
import { FormsModule }   from '@angular/forms';

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
        BrowserModule,
        FormsModule,
        HttpModule,
        CardsModule,
        routing
    ],
    declarations: [
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

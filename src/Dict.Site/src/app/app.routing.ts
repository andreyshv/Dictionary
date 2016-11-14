import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { DashboardComponent } from './dashboard.component';
import { CollectionsComponent } from './collections.component';
import { CollectionDetailComponent } from './collection-detail.component';

const appRoutes: Routes = [
    {
        path: '',
        redirectTo: '/collections',
        pathMatch: 'full'
    },
    {
        path: 'dashboard',
        component: DashboardComponent
    },
    {
        path: 'collections',
        component: CollectionsComponent
    },
    {
        path: 'collection-detail/:id',
        component: CollectionDetailComponent
    }
];

export const routing: ModuleWithProviders = RouterModule.forRoot(appRoutes); 
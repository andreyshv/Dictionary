import 'core-js/es6';
import 'core-js/es7/reflect';
require('zone.js/dist/zone');

// import 'rxjs/Observable';
// import 'rxjs/Subject';

// // Observable class extensions
// import 'rxjs/add/observable/of';
// import 'rxjs/add/observable/throw';

// // Observable operators
// import 'rxjs/add/operator/catch';
// import 'rxjs/add/operator/debounceTime';
// import 'rxjs/add/operator/distinctUntilChanged';
// import 'rxjs/add/operator/do';
// import 'rxjs/add/operator/filter';
// import 'rxjs/add/operator/map';
// import 'rxjs/add/operator/switchMap';
// import 'rxjs/add/operator/toPromise';

if (process.env.ENV === 'production') {
  // Production
} else {
  // Development
  Error['stackTraceLimit'] = Infinity;
  require('zone.js/dist/long-stack-trace-zone');
}

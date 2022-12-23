

import * as React from 'react';
import * as ReactDOM from 'react-dom';
import App from './App';
import './index.css';
import registerServiceWorker from './registerServiceWorker';
//import StatefulHello from './components/StatefulHello';


ReactDOM.render(
  <App/>,
  document.getElementById('root') as HTMLElement
);

/*
ReactDOM.render(
  <StatefulHello name="TypeScript" enthusiasmLevel={10} />,
  document.getElementById('root') as HTMLElement
);
*/
if(module.hot) {
  module.hot.accept();

}

registerServiceWorker();

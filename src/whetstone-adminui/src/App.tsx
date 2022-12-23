import * as React from 'react';
import './App.css';

//import logo from './logo.svg';

const list = [
 {
  title: 'React',
  url: 'https://facebook.github.io/react/',
  author: 'Jordan Walke',
  num_comments: 3,
  points: 4,
  objectId: 0
 },
 {
  title: 'Redux',
  url: 'https://facebook.github.io/reactjs/redux',
  author: 'Dan Abramov, Andew Clark',
  num_comments: 2,
  points: 5,
  objectId: 1
 }
];
class App extends React.Component {
  public render() {
 
    return ( 
      <div className="App">
      {list.map(item => 
          <div key={item.objectId}>
            <span>
              <a href={item.url}>{item.title}</a>
            </span>
            <span>{item.author}</span>
            <span>{item.num_comments}</span>
            <span>{item.points}</span>
          </div>      
      )}
      </div>
    );
  }
}


/*
class App extends React.Component {
  public render() {
    const helloWorld = "Welcome to the Road to Learn React";
    return ( 
      <div className="App">
        <header className="App-header">
          <img src={logo} className="App-logo" alt="logo" />
          <h1 className="App-title">{helloWorld}</h1>
        </header>
        <p className="App-intro">
          To get started, edit <code>src/App.tsx</code> and save to reload.
        </p>
      </div>
    );
  }
}
*/
export default App;

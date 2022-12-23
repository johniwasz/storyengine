export interface IEngineError {
  type: string;
  status: number;
  detail: string;
  errorCode: string;
  traceId: string;
  title: string;
}

export class UserNotConfirmed extends Error {
  constructor(m: string) {
    super(m);

    // Set the prototype explicitly.
    Object.setPrototypeOf(this, UserNotConfirmed.prototype);
    this.name = UserNotConfirmed.name;
  }
}

export class UserConfirmationCodeExpired extends Error {
  constructor(m: string) {
    super(m);

    // Set the prototype explicitly.
    Object.setPrototypeOf(this, UserConfirmationCodeExpired.prototype);
    this.name = UserConfirmationCodeExpired.name;
  }
}

export class UserNotAuthenticated extends Error {
  constructor(m: string) {
    super(m);

    // Set the prototype explicitly.
    Object.setPrototypeOf(this, UserNotAuthenticated.prototype);
    this.name = UserNotAuthenticated.name;
  }
}

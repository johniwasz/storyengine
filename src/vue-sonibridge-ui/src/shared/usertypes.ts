import { IUserSession, IUserCredentials, IAuthStatus, ISocketInfo } from '@/shared';

export interface IAuthManager {
  errorMessage: string;
  status: IAuthStatus;
  isConfirmed: boolean;
  userSession: IUserSession | undefined;
  credentials: IUserCredentials | undefined;
  signUpAsync(signUpRequest: ISignUpRequest): Promise<void>;
  signInAsync(creds: IUserCredentials): Promise<void>;
  signOutAsync(): Promise<void>;
}
/*
export interface IUserConfirmationManager {
  errorMessage: string;
  confirmUserAsync(confirmRequest: IUserConfirmationRequest): Promise<IUserConfirmationResponse>;
}
*/

export interface IUserConfirmationRequest {
  name: string;
  confirmationCode: string;
}

export interface INewUserConfirmationRequest {
  name: string;
}

export enum UserConfirmationStatus {
  confirmed = 'confirmed',
  expired = 'expired',
  invalid = 'invalid'
}

export interface IUserConfirmationResponse {
  status: UserConfirmationStatus;
}

export interface ISignUpRequest {
  name: string;
  password: string;
  acceptedTerms: boolean;
}

export interface ISignUpResponse {
  isSignedUp: string;
  code: string;
}

// This is returned by the API token sign in request at /api/tokens
export interface ITokenResponse {
  authToken: string | undefined;
  refreshToken: string | undefined;
  expiresIn: number | undefined;
  idToken: string | undefined;
  tokenType: string | undefined;
  permissions: string[] | undefined;
}

export interface ISignOutRequest {
  authToken: string;
}

export interface INotificationState {
  socketInfo: ISocketInfo;
}

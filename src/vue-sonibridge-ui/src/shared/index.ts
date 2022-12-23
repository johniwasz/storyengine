import AuthModule from '@/store/modules/authmanager';
import UserConfirmationManager from '@/store/modules/userconfirmationmanager';
import ProjectManager from '@/store/modules/projectmanager';
import VersionManager from '@/store/modules/versionmanager';

export * from './config';
export * from './constants';
export * from './data.service';
export * from './logger';
export * from './usertypes';
export * from './localuserstore';
export * from './errors';
export * from './permissions';

export type BotClient = 'unknown' | 'sms' | 'alexa' | 'googleHome';

export const BotClientOptions = [
  { value: 'unknown', text: 'Unknown' },
  { value: 'alexa', text: 'Alexa' },
  { value: 'googleHome', text: 'Google' },
  { value: 'sms', text: 'SMS/Text' }
];

export type VersionType = IVersion | undefined;

export type ProjectType = IProject | undefined;

export type DeploymentType = IDeployment | undefined;

// #region interfaces

export interface IProjectState {
  projects: IProject[];
  currentProject: ProjectType;
}

export interface IProject {
  id?: string;
  shortName?: string;
  description?: string;
}

export interface IVersionState {
  versions: IVersion[];
  currentVersion: VersionType;
}

export interface IVersion {
  id?: string;
  projectId?: string;
  version?: string;
  description?: string;
  logFullClientMessages?: boolean;
}

export interface IDeployment {
  id?: string;
  projectId?: string;
  versionId?: string;
  clientType: BotClient;
  clientId?: string;
  publishDate?: Date;
  alias?: string;
}

export interface IDeploymentState {
  deployments: IDeployment[];
  currentDeployment: DeploymentType;
}

export interface IAudioFileState {
  audioFiles: IAudioFile[];
  playbackInfo: IPlaybackAudioFile | undefined;
}

export interface IStoreType {
  authMod: AuthModule;
  userMod: UserConfirmationManager;
  projMod: ProjectManager;
  versionMod: VersionManager;
}

export interface IAuthState {
  errorMessage: string;
  status: IAuthStatus;
  isConfirmed: boolean;
  userSession: IUserSession | undefined;
  credentials: IUserCredentials | undefined;
}

export interface IUserSession {
  userName: string | undefined;
  authToken: string | undefined;
  refreshToken: string | undefined;
}

export interface IAuthStatus {
  loggingIn: boolean;
  isAuthenticated: boolean;
  isError: boolean;
}

export interface IUserCredentials {
  name: string | undefined;
  password: string | undefined;
}

export interface IAudioFile {
  fileName: string;
  size: number;
  lastModified?: Date;
}

export interface IDisplayAudioFile {
  fileName: string;
  size: number;
  lastModified?: string;
}
// #endregion

// #region Classes

export interface IAddDeploymentRequest {
  projectId: string;
  versionId: string;
  clientType: BotClient;
  clientId: string;
  alias?: string;
}

export interface IDeleteDeploymentRequest {
  projectId: string;
  versionId: string;
  deploymentId: string;
}

export interface IVersionUpdateRequest {
  description: string | undefined;
  logFullClientMessages: boolean | undefined;
}

export interface IVersionAudioFileRequest {
  projectId?: string;
  versionId?: string;
  fileName: string;
}

export interface IVersionAudioFileUploadRequest {
  projectId?: string;
  versionId?: string;
  file: File;
}

export interface IAudioFileTableField {
  key: string;
  label: string;
}

export interface IPlaybackAudioFile {
  fileName: string;
  file: Blob;
}

export interface IUniqueId {
  id: string;
}

export interface ISocketInfo {
  clientId: string | undefined;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  socket: any;
}

export interface IOpenSocketRequest {
  url: string;
  authToken?: string;
  apiKey: string;
  clientId?: string;
}

// #endregion

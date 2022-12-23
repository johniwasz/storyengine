import Axios, * as axios from 'axios';

const AuthHeader = 'Authorization';
const ApiKeyHeader = 'x-api-key';

// const SEC_UNEXPECTED_ERROR = 'SEC001';
const USER_NOT_AUTHENTICATED = 'SEC002';
const USER_NOT_CONFIRMED = 'SEC003';
// const TOKEN_VALIDATION_FAILED = 'SEC004';
const USER_CONFIRMATION_CODE_EXPIRED = 'SEC005';
// const TOKEN_PARSING_FAILED = 'SEC006';

import {
  IProject,
  IVersionUpdateRequest,
  IVersion,
  IDeployment,
  IAddDeploymentRequest,
  IAudioFile,
  IUserCredentials,
  ITokenResponse,
  ISignOutRequest,
  ISignUpRequest,
  ISignUpResponse,
  IUserConfirmationRequest,
  IUserConfirmationResponse,
  localUserStore,
  BotClient,
  UserNotConfirmed,
  UserNotAuthenticated,
  UserConfirmationCodeExpired,
  IEngineError,
  IUniqueId,
  logger
} from '@/shared';
import { API, APIKEY } from '@/shared/config';
import { INewUserConfirmationRequest } from './usertypes';
// import { BIconFileEarmarkSpreadsheet } from 'bootstrap-vue';

interface IRefreshTokenRequest {
  refreshToken: string;
}

Axios.interceptors.request.use(
  (config) => {
    Axios.defaults.headers.common[ApiKeyHeader] = APIKEY;

    const authToken = localUserStore.getAuthToken();

    if (authToken) {
      config.headers[AuthHeader] = `Bearer ${authToken}`;
    }

    return config;
  },
  (error) => {
    Promise.reject(error);
  }
);

interface IAxiosRequestConfigExtended extends axios.AxiosRequestConfig {
  retry: boolean;
}

Axios.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    const axError: axios.AxiosError = error as axios.AxiosError;

    if (axError) {
      const userStore = localUserStore.getUserStore();

      const reqExtended: IAxiosRequestConfigExtended = axError.request;

      if (error.response?.status === 401 && !reqExtended.retry && userStore?.refreshToken) {
        reqExtended.retry = true;

        const refRequest: IRefreshTokenRequest = { refreshToken: userStore.refreshToken };

        Axios.post<ITokenResponse>(`${API}/token/refresh`, refRequest).then((res) => {
          if (res.status === 200 && res.data.authToken) {
            // 1) put token to LocalStorage
            localUserStore.setAuthToken(res.data.authToken);

            // 2) Change Authorization header
            Axios.defaults.headers.common[AuthHeader] = `Bearer ${res.data.authToken}`;

            // 3) return originalRequest object with Axios.
            Promise.resolve(Axios(reqExtended));
          }
        });
      } else {
        if (axError?.response) {
          const engineErr: IEngineError = axError.response.data as IEngineError;
          if (engineErr) {
            switch (engineErr.errorCode) {
              case USER_NOT_CONFIRMED:
                throw new UserNotConfirmed(engineErr.detail);
              case USER_NOT_AUTHENTICATED:
                throw new UserNotAuthenticated(engineErr.detail);
              case USER_CONFIRMATION_CODE_EXPIRED:
                throw new UserConfirmationCodeExpired(engineErr.detail);
              default:
                throw new Error(engineErr.detail);
            }
          } else if (axError.response.data?.message) {
            throw Error(axError.response.data.message);
          } else throw Error(axError.response.statusText);
        }
      }
    }
    // return Error object with Promise
    throw error;
  }
);
interface IAddDeploymentRequestInternal {
  clientType: BotClient;
  clientId: string;
  alias?: string;
}

export class DataService {
  public async signInAsync(creds: IUserCredentials): Promise<ITokenResponse | undefined> {
    const response: axios.AxiosResponse<ITokenResponse> = await Axios.post<ITokenResponse>(`${API}/token`, creds);
    const tokenResponse = response.data;
    return tokenResponse;
  }

  public async signUpAsync(signUpRequest: ISignUpRequest): Promise<void> {
    await Axios.post<ISignUpResponse>(`${API}/user/signup`, signUpRequest);
  }

  public async confirmUserAsync(confirmationRequest: IUserConfirmationRequest): Promise<IUserConfirmationResponse> {
    try {
      const response: axios.AxiosResponse<IUserConfirmationResponse> = await Axios.post<IUserConfirmationResponse>(
        `${API}/user/confirm`,
        confirmationRequest
      );
      if (response.status !== 200) throw Error(response.statusText);

      if (response.data) {
        const confirmationResponse = response.data;
        return confirmationResponse;
      } else throw Error(`Unexpected data response from API `);
    } catch (error) {
      throw new Error(`Error confirming user: ${error.message}`);
    }
  }

  public async requestNewUserConfirmationCode(newConfirmationCodeRequest: INewUserConfirmationRequest): Promise<void> {
    let response: axios.AxiosResponse<void>;

    try {
      response = await Axios.post<void>(`${API}/user/newconfirmcode`, newConfirmationCodeRequest);

      // expecting a 202 Accepted response
      if (response.status !== 202) throw Error(response.statusText);
    } catch (error) {
      throw new Error(`Error requesting new confirmation code: ${error.message}`);
    }
  }

  public async signOutAsync(signOutReq: ISignOutRequest): Promise<void> {
    try {
      logger.log(`${API}/token/signout`);
      const response: axios.AxiosResponse<ITokenResponse> = await Axios.post<ITokenResponse>(
        `${API}/token/signout`,
        signOutReq
      );

      if (response.status !== 200) throw Error(response.statusText);
    } catch (error) {
      logger.info('Error in signOutAsync', error);
    }
  }

  public async getProjectsAsync(): Promise<IProject[]> {
    try {
      const response: axios.AxiosResponse<IProject[]> = await Axios.get<IProject[]>(`${API}/project`);
      const projects = response.data;
      return projects;
    } catch (error) {
      throw new Error(`Error getting projects: ${error.message}`);
    }
  }

  public async getDeploymentsAsync(projectId: string, versionId: string): Promise<IDeployment[]> {
    try {
      const url = `${API}/project/${projectId}/version/${versionId}/deployment`;

      const response: axios.AxiosResponse<IDeployment[]> = await Axios.get<IDeployment[]>(url);

      if (response.status !== 200) throw Error(response.statusText);
      const deployments = response.data;

      return deployments;
    } catch (error) {
      logger.info('error in getDeploymentsAsync', error);

      // Return an empty project list if an error occurs.
      const deployments: IDeployment[] = [];
      return deployments;
    }
  }

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  /*   public async saveDeployment(deploymentToSave: IDeployment): Promise<IDeployment> {
    return new Deployment();
  } */

  public async deleteDeploymentAsync(projectId: string, versionId: string, deploymentId: string): Promise<void> {
    try {
      const url = `${API}/project/${projectId}/version/${versionId}/deployment/${deploymentId}`;

      const response: axios.AxiosResponse = await Axios.delete(url);

      if (response.status !== 200) throw Error(response.statusText);
    } catch (error) {
      logger.info('error in deleteDeploymentsAsync', error);

      // Return an empty project list if an error occurs.
    }
  }

  public async saveVersion(versionToSave: IVersion): Promise<IVersion> {
    let retVersion: IVersion;

    const updateRequest: IVersionUpdateRequest = {
      description: versionToSave.description,
      logFullClientMessages: versionToSave.logFullClientMessages
    };

    try {
      const response: axios.AxiosResponse<IVersion> = await Axios.put<IVersion>(
        `${API}/project/${versionToSave.projectId}/version/${versionToSave.id}`,
        updateRequest
      );

      if (response.status !== 200) throw Error(response.statusText);
      const savedVersion = response.data;

      return savedVersion;
    } catch (error) {
      logger.info('error in saveVersion', error);

      retVersion = {
        id: undefined,
        description: undefined,
        logFullClientMessages: undefined,
        projectId: undefined,
        version: undefined
      };

      return retVersion;
    }
  }

  public async addDeploymentAsync(newDeployment: IAddDeploymentRequest): Promise<IDeployment> {
    try {
      const internalRequest: IAddDeploymentRequestInternal = {
        clientId: newDeployment.clientId,
        clientType: newDeployment.clientType,
        alias: newDeployment.alias
      };

      const response: axios.AxiosResponse<IDeployment> = await Axios.post<IDeployment>(
        `${API}/project/${newDeployment.projectId}/version/${newDeployment.versionId}/deployment`,
        internalRequest
      );

      if (response.status !== 200) throw Error(response.statusText);
      const deployment = response.data;
      return deployment;
    } catch (error) {
      logger.info('error in addDeploymentAsync', error);

      // Return an empty project list if an error occurs.
      const deployment: IDeployment = {
        clientType: 'unknown',
        alias: undefined,
        clientId: undefined,
        id: undefined,
        projectId: undefined,
        publishDate: undefined,
        versionId: undefined
      };
      return deployment;
    }
  }

  public async getVersionsAsync(projectId: string): Promise<IVersion[]> {
    try {
      const response: axios.AxiosResponse<IVersion[]> = await Axios.get<IVersion[]>(
        `${API}/project/${projectId}/version`
      );

      if (response.status !== 200) throw Error(response.statusText);
      const versions = response.data;
      return versions;
    } catch (error) {
      logger.info('error in getVersionsAsync', error);

      // Return an empty project list if an error occurs.
      const versions: IVersion[] = [];
      return versions;
    }
  }

  public async getAudioFilesAsync(projectId: string, versionId: string): Promise<IAudioFile[]> {
    try {
      logger.log(`${API}/project/${projectId}/version/${versionId}/audio`);
      const response: axios.AxiosResponse<IAudioFile[]> = await Axios.get<IAudioFile[]>(`
        ${API}/project/${projectId}/version/${versionId}/audio`);

      if (response.status !== 200) throw Error(response.statusText);
      const audioFiles = response.data;
      return audioFiles;
    } catch (error) {
      logger.info('error in getAudioFilesAsync', error);

      // Return an empty project list if an error occurs.
      const audioFiles: IAudioFile[] = [];
      return audioFiles;
    }
  }

  public async getAudioFileBinaryAsync(projectId: string, versionId: string, fileName: string): Promise<Blob> {
    try {
      logger.log(`${API}/project/${projectId}/version/${versionId}/audio/${fileName}/content`);
      const cfg: axios.AxiosRequestConfig = {
        responseType: 'blob',
        headers: {
          Accept: 'audio/mpeg'
        }
      };
      const response: axios.AxiosResponse = await Axios.get(
        `${API}/project/${projectId}/version/${versionId}/audio/${fileName}/content`,
        cfg
      );
      if (response.status !== 200) throw Error(response.statusText);
      const audioBlob = new Blob([response.data], { type: 'audio/mpeg' });
      return audioBlob;
    } catch (error) {
      logger.info('Error in getAudioFileBinaryAsync', error);

      // Return an empty blob if an error occurs.
      const audioBlob: Blob = new Blob();
      return audioBlob;
    }
  }

  public async uploadAudioFileAsync(projectId: string, versionId: string, file: File): Promise<IAudioFile | null> {
    try {
      const fileName = file.name;
      const formData = new FormData();
      formData.set('UploadedFile', file);

      logger.log(`${API}/project/${projectId}/version/${versionId}/audio/${fileName}/content`);
      const cfg: axios.AxiosRequestConfig = {
        headers: {
          'Content-Type': 'multipart/form-data'
        }
      };

      const response: axios.AxiosResponse = await Axios.post(
        `${API}/project/${projectId}/version/${versionId}/audio/${fileName}/content`,
        formData,
        cfg
      );

      if (response.status !== 200) throw Error(response.statusText);
      const audioFile = response.data;
      return audioFile;
    } catch (error) {
      logger.info('error in uploadAudioFileAsync', error);

      // Return an empty blob if an error occurs.
      return null;
    }
  }

  public async deleteAudioFileAsync(projectId: string, versionId: string, fileName: string): Promise<boolean> {
    try {
      logger.log(`Deleting file: ${fileName} for Project ID: ${projectId} and Version ID: ${versionId}`);

      const response: axios.AxiosResponse = await Axios.delete(
        `${API}/project/${projectId}/version/${versionId}/audio/${fileName}`
      );

      if (response.status !== 200) throw Error(response.statusText);

      return true;
    } catch (error) {
      logger.info('error in deleteAudioFileAsync', error);
      return false;
    }
  }

  public async getUniqueId(): Promise<IUniqueId | undefined> {
    try {
      const response: axios.AxiosResponse<IUniqueId> = await Axios.get<IUniqueId>(`${API}/uniqueid`);
      const uniqueId = response.data;
      return uniqueId;
    } catch (error) {
      logger.info('error in getClientId', error);
      return undefined;
    }
  }
}

export const dataService: DataService = new DataService();

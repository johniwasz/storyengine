import store from '@/store';

import { VuexModule, Module, Mutation, Action, getModule } from 'vuex-module-decorators';

import { SET_CLIENTID, SET_SOCKET } from '@/store/mutation-types';

import { dataService, INotificationState, ISocketInfo, IUniqueId, logger, IOpenSocketRequest } from '@/shared';

// import store from '@/store';

const EMPTY_SOCKETINFO: ISocketInfo = {
  clientId: undefined,
  socket: null
};

@Module({ dynamic: true, name: 'notificationMod', store })
export class NotificationManager extends VuexModule implements INotificationState {
  public socketInfo: ISocketInfo = EMPTY_SOCKETINFO;

  @Action({ rawError: true })
  public async getClientId() {
    const uniqueId: IUniqueId | undefined = await dataService.getUniqueId();

    if (uniqueId !== undefined) {
      logger.log(`NotificationManager new clientId: ${uniqueId.id}`);
      this.context.commit(SET_CLIENTID, uniqueId.id);
    }
  }

  @Action({ rawError: true })
  public async clearClientId() {
    this.context.commit(SET_CLIENTID, undefined);
  }

  @Action({ rawError: true })
  public async openSocket(request: IOpenSocketRequest) {
    if (request.authToken !== undefined && request.clientId !== undefined) {
      const wsUri =
        request.url +
        '?' +
        'auth=' +
        encodeURIComponent(request.authToken) +
        '&api=' +
        encodeURIComponent(request.apiKey) +
        '&clientId=' +
        encodeURIComponent(request.clientId);

      logger.log(`Connecting to socket uri: ${wsUri}`);

      const socket = new WebSocket(wsUri);

      socket.onopen = (e) => {
        logger.log('socket opened' + JSON.stringify(e));
        this.context.commit(SET_SOCKET, socket);
      };

      socket.onclose = (e) => {
        logger.log('socket closed' + JSON.stringify(e));
        this.context.commit(SET_SOCKET, null);
      };

      socket.onmessage = (e) => {
        logger.log('on message' + JSON.stringify(e));
      };

      socket.onerror = (e) => {
        logger.log('error' + JSON.stringify(e));
      };
    }
  }

  @Mutation
  // tslint:disable-next-line
  private [SET_CLIENTID](newClientId: string | undefined): void {
    logger.log('Updating client id to: ' + newClientId);
    this.socketInfo.clientId = newClientId;
  }

  @Mutation
  // eslint-disable-next-line
  // tslint:disable-next-line
  private [SET_SOCKET](newSocket: any): void {
    logger.log('Updating socket to: ' + newSocket);
    this.socketInfo.socket = newSocket;
  }
}

export const NotificationModule = getModule(NotificationManager);

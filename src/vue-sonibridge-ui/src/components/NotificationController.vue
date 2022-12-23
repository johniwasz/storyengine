<template>
  <div></div>
</template>

<script lang="ts">
import { Component, Vue, Watch } from 'vue-property-decorator';
import { AuthModule } from '@/store/modules/authmanager';
import { NotificationModule } from '@/store/modules/notificationmanager';
import { IOpenSocketRequest, IUserSession, logger } from '@/shared';
import { SOCKETAPI, SOCKETAPIKEY } from '@/shared/config';

@Component({ name: 'NotificationController' })
export default class NotificationController extends Vue {
  get IsAuthenticated(): boolean {
    return AuthModule.status.isAuthenticated;
  }

  get IsSocketConnected(): boolean {
    return NotificationModule.socketInfo.socket != null;
  }

  get AuthToken(): string | undefined {
    return AuthModule.userSession.authToken;
  }

  get ClientId(): string | undefined {
    return NotificationModule.socketInfo.clientId;
  }

  get UserSession(): IUserSession {
    return AuthModule.userSession;
  }

  @Watch('IsAuthenticated', { immediate: true })
  public async onAuthenticatedChanged(val: boolean, oldVal: boolean) {
    logger.log(`NotificationController onAuthenticatedChanged: ${val}`);

    if (val && val !== oldVal) {
      if (this.ClientId === undefined) {
        await NotificationModule.getClientId();
      }
    }
  }

  @Watch('ClientId', { immediate: true })
  // eslint-disable-next-line
  public async onClientIDChanged(val: string | undefined, oldVal: string | undefined) {
    if (val !== undefined) {
      // await this.OpenSocket();
    }
  }

  @Watch('IsSocketConnected', { immediate: true })
  // eslint-disable-next-line
  public async onSocketConnectedChanged(val: boolean, oldVal: boolean) {
    logger.log(`Notification Controller::IsSocketConnected val: ${val}`);
    logger.log(`Notification Controller::IsSocketConnected oldVal: ${oldVal}`);
  }

  private async OpenSocket() {
    if (!this.IsSocketConnected) {
      const request: IOpenSocketRequest = {
        url: SOCKETAPI,
        authToken: this.AuthToken,
        apiKey: SOCKETAPIKEY,
        clientId: this.ClientId
      };

      await NotificationModule.openSocket(request);
    }
  }
}
</script>

<template>
  <div class="user-form">
    <ValidationObserver v-slot="{ invalid }" tag="form" @submit="handleSubmit">
      <b-container fluid="true">
        <b-row class="m-2">
          <b-col><h3>Validate Confirmation Code</h3></b-col>
        </b-row>

        <b-row class="m-2">
          <b-col
            ><div v-visible="!isRedirecting" :class="{ errortext: !isCodeValid && isVerified }">{{ instructions }}</div>
            <div v-visible="isRedirecting && !isLoggedIn">
              <p>Confirmation successful. Redirecting to <b-link to="/login">Sign in</b-link> in</p>
              <TextTimer ref="textTimer" @countdownDone="redirectConfirmation" countDown="10"></TextTimer>
            </div>
            <div v-visible="isRedirecting && isLoggedIn">
              <p>Confirmation successful. Redirecting to <b-link to="/">Home</b-link> in</p>
              <TextTimer ref="textTimer" @countdownDone="redirectConfirmation" countDown="10"></TextTimer>
            </div>
          </b-col>
        </b-row>
        <b-form-row class="m-2">
          <b-col>
            <ValidatingInput
              name="txtLogin"
              v-model="userNameVal"
              fieldName="user name"
              displayIcon="person-fill"
              :rules="emailRules"
              placeholder="email"
              @validated="emailValidated"
              :readonly="isVerified"
              >{{ userNameVal }}</ValidatingInput
            >
          </b-col>
        </b-form-row>
        <b-form-row class="m-2">
          <b-col>
            <ValidatingInput
              name="txtConfirmationCode"
              v-model="confirmationCode"
              fieldName="confirmationCode"
              displayIcon="lock-fill"
              :rules="requiredRule"
              placeholder="confirmation code"
              >{{ confirmationCode }}</ValidatingInput
            >
          </b-col>
        </b-form-row>
        <b-form-row class="m-2">
          <b-col>
            <i class="glyphicon glyphicon-none"></i
            ><b-button :disabled="invalid || confirmingCode" type="submit" id="btnLogin" variant="primary"
              >Validate Code</b-button
            >
          </b-col>
        </b-form-row>

        <b-form-row class="m-2">
          <b-col>
            <b-spinner small v-visible="confirmingCode" variant="primary"></b-spinner>
          </b-col>
        </b-form-row>
        <b-row>
          <b-col>
            <div class="text-center">
              <b-link to="/register">Create</b-link> a new account or <b-link to="/login">Sign in</b-link><br /><br />
            </div>
          </b-col>
        </b-row>

        <b-form-row class="m-2">
          <b-col>
            <i class="glyphicon glyphicon-none"></i
            ><b-button id="btnNewCode" :disabled="!isEmailValid" variant="primary" @click="requestNewCode()"
              >Request New Code</b-button
            >
          </b-col>
        </b-form-row>
        <b-row class="m-2" v-visible="confirmationResent">
          <b-col> <i class="glyphicon glyphicon-none" />A new confirmation code has been sent to {{ userName }} </b-col>
        </b-row>
      </b-container>
    </ValidationObserver>
    <div v-visible="isError" class="alert alert-danger">{{ currentError }}</div>
  </div>
</template>

<script lang="ts">
// import { Auth, AuthOptions } from '@aws-amplify/auth';
import { Component, Prop, Ref } from 'vue-property-decorator';
import { AuthModule } from '@/store/modules/authmanager';
import { UserConfirmationModule } from '@/store/modules/userconfirmationmanager';
import {
  logger,
  IUserConfirmationRequest,
  IUserConfirmationResponse,
  INewUserConfirmationRequest,
  UserConfirmationStatus
} from '@/shared';
import { ValidationProvider, ValidationObserver } from 'vee-validate';
import ValidatingInput from '@/components/forms/ValidatingInput.vue';
import { SecurityMixin } from '@/components/security/SecurityMixin';
import VueRouter from 'vue-router';
import { Mixins } from 'vue-mixin-decorator';
import TextTimer from '@/components/general/TextTimer.vue';

@Component({
  name: 'ConfirmationCode',
  components: { ValidationProvider, ValidationObserver, ValidatingInput, TextTimer }
})
export default class ConfirmationCode extends Mixins<SecurityMixin>(SecurityMixin) {
  @Prop()
  public userName!: string | undefined;

  public userNameVal = this.userName === undefined ? '' : this.userName;

  @Ref()
  protected textTimer!: TextTimer;

  protected confirmationCode = '';

  protected currentError = '';

  protected isError = false;

  protected confirmingCode = false;

  protected isVerified = false;

  protected isEmailValid = false;

  protected confirmationResent = false;

  protected isRedirecting = false;

  protected isCodeValid = false;

  protected isLoggedIn = false;

  protected redirectDest!: string;

  protected instructions = 'Please enter your email and the confirmation code emailed to you';

  protected async handleSubmit(evt: Event): Promise<void> {
    evt.preventDefault();

    const confReq: IUserConfirmationRequest = { name: this.userNameVal, confirmationCode: this.confirmationCode };
    console.log('submitted');

    if (confReq.name && confReq.confirmationCode) {
      this.currentError = '';
      this.isError = false;
      this.confirmingCode = true;

      try {
        const confResp: IUserConfirmationResponse = await UserConfirmationModule.confirmUserAsync(confReq);

        switch (confResp.status) {
          case UserConfirmationStatus.confirmed:
            this.isVerified = true;
            this.isCodeValid = true;
            await this.processPostConfirmationAsync();
            break;
          case UserConfirmationStatus.expired:
            this.isVerified = true;
            this.isCodeValid = false;
            this.instructions = 'Your confirmation code has expired. You may request a new confirmation code.';
            break;
          case UserConfirmationStatus.invalid:
            this.isVerified = true;
            this.isCodeValid = false;
            this.instructions =
              'Confirmation code is invalid. If you do not have access to your confirmation code, you may request a new code.';
            break;
        }
      } catch (err) {
        this.isError = true;
        this.currentError = err.message;
      } finally {
        this.confirmingCode = false;
      }
    } else {
      console.log(confReq);
      this.isError = true;
      this.currentError = 'invalid confirmation request';
    }
  }

  protected async processPostConfirmationAsync(): Promise<void> {
    this.isLoggedIn = false;

    AuthModule.updateUserName(this.userNameVal);

    // if the user name and password are set, then attempt to log in.
    if (AuthModule.credentials?.name && AuthModule.credentials?.password) {
      // if login fails, then redirect back to login

      try {
        await AuthModule.autoSignInAsync();

        if (AuthModule.status?.isAuthenticated === true) this.isLoggedIn = true;
      } catch (ex) {
        logger.log(ex.message);
      }
    }

    this.redirectDest = this.isLoggedIn ? '/' : '/login';

    this.isRedirecting = true;
    this.textTimer.startCountdown();
  }

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  protected redirectConfirmation(evt: Event) {
    const router: VueRouter = this.$router as VueRouter;
    router.push(this.redirectDest);
  }

  protected emailValidated(valid: boolean): void {
    this.isEmailValid = valid;
  }

  protected async requestNewCode(): Promise<void> {
    if (this.userName) {
      const confReq: INewUserConfirmationRequest = { name: this.userNameVal };

      try {
        await UserConfirmationModule.requestNewCode(confReq);
        this.confirmationResent = true;
        // display success message
      } catch (err) {
        this.isError = true;
        this.confirmationResent = false;
        this.currentError = err.message;
      }
    }
  }
}
</script>

<style scoped lang="scss">
@import '@/design/userforms.scss';
</style>

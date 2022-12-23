<template>
  <div class="user-form">
    <ValidationObserver v-slot="{ invalid }" tag="form" @submit="handleSubmit">
      <b-container fluid="true">
        <b-row class="m-2">
          <b-col><h2>Sign In</h2></b-col>
        </b-row>
        <b-form-row class="m-2">
          <b-col>
            <ValidatingInput
              name="txtLogin"
              v-model="userName"
              fieldName="user name"
              displayIcon="person-fill"
              :rules="emailRules"
              placeholder="email"
              @validated="userNameValidated"
              >{{ userName }}</ValidatingInput
            >
          </b-col>
        </b-form-row>
        <b-form-row class="m-2">
          <b-col>
            <ValidatingInput
              name="txtPassword"
              v-model="password"
              fieldName="password"
              displayIcon="lock-fill"
              :rules="requiredRule"
              placeholder="password"
              type="password"
              @validated="passwordValidated"
              >{{ password }}</ValidatingInput
            >
          </b-col>
        </b-form-row>
        <b-form-row class="m-2">
          <b-col>
            <i class="glyphicon glyphicon-none"></i
            ><b-button :disabled="invalid" type="submit" variant="primary">Login</b-button>
          </b-col>
        </b-form-row>
        <b-form-row class="m-2">
          <b-col>
            <b-spinner small v-visible="loginStatus.loggingIn" variant="primary"></b-spinner>
          </b-col>
        </b-form-row>
        <b-row>
          <b-col>
            <div class="text-center"><b-link to="/register">Create</b-link> a new account</div>
          </b-col>
        </b-row>
      </b-container>
    </ValidationObserver>
    <div v-visible="loginStatus.isError" class="alert alert-danger">{{ currentError }}</div>
  </div>
</template>

<script lang="ts">
// import Vue from 'vue';
// import { Auth, AuthOptions } from '@aws-amplify/auth';
import { Component } from 'vue-property-decorator';
// import { authStore } from '@/store';
import { AuthModule } from '@/store/modules/authmanager';
import { IAuthStatus } from '@/shared';
import VueRouter, { RawLocation } from 'vue-router';
import { ValidationProvider, ValidationObserver } from 'vee-validate';
import ValidatingInput from '@/components/forms/ValidatingInput.vue';
import { SecurityMixin } from '@/components/security/SecurityMixin';
import { Mixins } from 'vue-mixin-decorator';

@Component({
  name: 'FrontDoor',
  components: { ValidationProvider, ValidationObserver, ValidatingInput }
})
export default class FrontDoor extends Mixins<SecurityMixin>(SecurityMixin) {
  public userName: string = this.getCurrentUserName();

  public password = this.getCurrentPassword();

  public submitted = false;

  public async handleSubmit(evt: Event): Promise<void> {
    evt.preventDefault();
    this.submitted = true;

    await AuthModule.autoSignInAsync();
    const router: VueRouter = this.$router as VueRouter;
    if (this.loginStatus.isAuthenticated) {
      const route: RawLocation = this.$route?.query?.redirect as RawLocation;
      router.push(route || '/');
    } else {
      // If the user is not confirmed and no error occurred, then route the user to the confirmation view
      if (!this.isConfirmed && !this.loginStatus.isError) {
        router.push({ path: '/confirmuser', query: { userName: this.userName } });
      }
    }
  }

  public userNameValidated(isValid: boolean): void {
    if (isValid) AuthModule.updateUserName(this.userName);
    else AuthModule.updateUserName(undefined);
  }

  public passwordValidated(isValid: boolean): void {
    if (isValid) AuthModule.updatePassword(this.password);
    else AuthModule.updatePassword(undefined);
  }

  public async createAccount(evt: Event): Promise<void> {
    evt.preventDefault();
    const router: VueRouter = this.$router as VueRouter;
    const location: RawLocation = '/register';

    router.push(location);
  }

  get loginStatus(): IAuthStatus {
    const authStatus = AuthModule.status;

    return authStatus;
  }

  get isConfirmed(): boolean {
    return AuthModule.isConfirmed;
  }

  get currentError(): string {
    return AuthModule.errorMessage;
  }

  private getCurrentPassword() {
    if (AuthModule?.credentials?.password) return AuthModule.credentials.password;
    else return '';
  }

  private getCurrentUserName() {
    if (AuthModule?.credentials?.name) return AuthModule.credentials.name;
    else return '';
  }
}
</script>

<style scoped lang="scss">
@import '@/design/userforms.scss';
</style>

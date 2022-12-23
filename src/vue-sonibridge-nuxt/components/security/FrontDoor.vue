<template>
  <div class="user-form">
    <ValidationObserver v-slot="{ invalid }" tag="form" @submit="handleSubmit">
      <b-container fluid="true">
        <b-row class="m-2">
          <b-col>
            <h2>Sign In</h2>
          </b-col>
        </b-row>
        <b-form-row class="m-2">
          <b-col>
            <ValidatingInput
              v-model="userName"
              name="txtLogin"
              field-name="user name"
              display-icon="person-fill"
              :rules="emailRules"
              placeholder="email"
            >{{ userName }}</ValidatingInput>
          </b-col>
        </b-form-row>
        <b-form-row class="m-2">
          <b-col>
            <ValidatingInput
              v-model="password"
              name="txtPassword"
              field-name="password"
              display-icon="lock-fill"
              :rules="requiredRule"
              placeholder="password"
              type="password"
            >{{ password }}</ValidatingInput>
          </b-col>
        </b-form-row>
        <b-form-row class="m-2">
          <b-col>
            <i class="glyphicon glyphicon-none"></i>
            <b-button :disabled="invalid" type="submit" variant="primary">Login</b-button>
          </b-col>
        </b-form-row>
        <b-form-row class="m-2">
          <b-col>
            <b-spinner v-visible="loginStatus.loggingIn" small variant="primary"></b-spinner>
          </b-col>
        </b-form-row>
        <b-row>
          <b-col>
            <div class="text-center">
              <b-link to="/register">Create</b-link>a new account
            </div>
          </b-col>
        </b-row>
      </b-container>
    </ValidationObserver>
    <div v-visible="loginStatus.isError" class="alert alert-danger">{{ currentError }}</div>
  </div>
</template>

<script lang="ts">
// import { Auth, AuthOptions } from '@aws-amplify/auth';
import { Component } from 'vue-property-decorator'
import VueRouter, { RawLocation } from 'vue-router'
import { ValidationProvider, ValidationObserver } from 'vee-validate'
import { Mixins } from 'vue-mixin-decorator'
import { authStore } from '@/store'
// import { AuthModule } from '@/store/modules/authmanager';
import { IUserCredentials, IAuthStatus, logger } from '@/shared'
import ValidatingInput from '@/components/forms/ValidatingInput.vue'
import { SecurityMixin } from '@/components/security/SecurityMixin'

@Component({
  name: 'FrontDoor',
  components: { ValidationProvider, ValidationObserver, ValidatingInput }
})
export default class FrontDoor extends Mixins<SecurityMixin>(SecurityMixin) {
  public userName = ''

  public password = ''

  public submitted = false

  public async handleSubmit(evt: Event): Promise<void> {
    const creds: IUserCredentials = {
      name: this.userName,
      password: this.password
    }
    evt.preventDefault()
    this.submitted = true

    if (creds.name && creds.password) {
      await authStore.signInAsync(creds)
      const router: VueRouter = this.$router as VueRouter
      if (this.loginStatus.isAuthenticated) {
        const route: RawLocation = this.$route?.query?.redirect as RawLocation
        router.push(route || '/')
      } else {
        // If the user is not confirmed and no error occurred, then route the user to the confirmation view
        if (!this.isConfirmed && !this.loginStatus.isError) {
          router.push({
            path: '/confirmuser',
            query: { userName: this.userName }
          })
        }
      }
    }
  }

  public async createAccount(evt: Event): Promise<void> {
    evt.preventDefault()
    const router: VueRouter = this.$router as VueRouter
    const location: RawLocation = '/register'

    router.push(location)
  }

  get loginStatus(): IAuthStatus {
    const authStatus = authStore.status

    return authStatus
  }

  get isConfirmed(): boolean {
    return authStore.isConfirmed
  }

  get currentError(): string {
    return authStore.errorMessage
  }

  protected mounted() {
    logger.log('Front door mounted')
    if (authStore.credentials?.name) {
      logger.log('set user name')
      this.userName = authStore.credentials.name
    }

    if (authStore.credentials?.password) {
      this.password = authStore.credentials.password
    }
  }
}
</script>

<style scoped lang="scss">
@import '@/design/userforms.scss';
</style>

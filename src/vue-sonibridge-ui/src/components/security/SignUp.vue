<template>
  <div class="user-form">
    <ValidationObserver v-slot="{ invalid, errors }" tag="form" @submit="handleSubmit">
      <b-container fluid="true">
        <b-row class="m-2">
          <b-col>
            <h2>Sign Up</h2>
            <p>Please fill in this form to create an account!</p>
            <hr
          /></b-col>
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
              >{{ userName }}</ValidatingInput
            >
          </b-col>
        </b-form-row>
        <ValidationObserver>
          <b-form-row class="m-2">
            <b-col>
              <ValidationProvider
                name="password"
                ref="valPasswordProvider"
                :rules="passwordConfirmRule('@confirmpassword')"
              >
                <b-form-group>
                  <div class="input-group">
                    <b-icon icon="lock-fill" class="align-self-center" />

                    <b-form-input
                      ref="txtPassword"
                      align-h="start"
                      id="txtPassword"
                      v-model="password"
                      type="password"
                      :lazy="true"
                      placeholder="password"
                      @change="validatePassword(1)"
                      autocomplete="off"
                      :state="isPasswordConfirmed"
                    />
                  </div>
                </b-form-group>
              </ValidationProvider>
            </b-col>
          </b-form-row>

          <b-form-row class="m-2">
            <b-col>
              <ValidationProvider
                name="confirmpassword"
                ref="valConfirmPasswordProvider"
                :rules="passwordConfirmRule('@password')"
              >
                <b-form-group>
                  <div class="input-group">
                    <b-icon icon="lock-fill" class="align-self-center" />

                    <b-form-input
                      ref="txtConfirmPassword"
                      align-h="start"
                      id="txtConfirmPassword"
                      v-model="confirmPassword"
                      type="password"
                      :lazy="true"
                      @change="validatePassword(2)"
                      placeholder="confirm Password"
                      autocomplete="off"
                      :state="isPasswordConfirmed"
                    />
                  </div>
                  <b-form-invalid-feedback :state="isPasswordConfirmed">{{
                    passwordErrorText
                  }}</b-form-invalid-feedback>
                </b-form-group>
              </ValidationProvider>
            </b-col>
          </b-form-row>
        </ValidationObserver>
        <b-form-row>
          <b-col>
            <ValidationProvider name="confirmTerms">
              <b-form-checkbox
                type="checkbox"
                id="acceptterms"
                name="acceptterms"
                value="accepted"
                v-model="termsStatus"
                unchecked-value="not_accepted"
                required="required"
              >
                I accept the <a href="#">Terms of Use</a> &amp; <a href="#">Privacy Policy</a></b-form-checkbox
              >
              <b-form-invalid-feedback v-visible="displayPasswordError" :state="displayPasswordError">{{
                errors[0]
              }}</b-form-invalid-feedback>
            </ValidationProvider>
          </b-col>
        </b-form-row>

        <b-form-row class="m-2">
          <b-col>
            <i class="glyphicon glyphicon-none"></i
            ><b-button :disabled="invalid" type="submit" variant="primary">Sign Up</b-button>
          </b-col>
        </b-form-row>
        <b-form-row class="m-2">
          <b-col>
            <b-spinner small v-visible="loginStatus.loggingIn" variant="primary"></b-spinner>
          </b-col>
        </b-form-row>
        <b-row>
          <b-col>
            <div class="text-center">Already have an account? <b-link to="/login">Login</b-link></div>
          </b-col>
        </b-row>
      </b-container>
    </ValidationObserver>
    <div v-visible="loginStatus.isError" class="alert alert-danger">{{ currentError }}</div>
  </div>
</template>

<script lang="ts">
// import { Auth, AuthOptions } from '@aws-amplify/auth';
import { Component } from 'vue-property-decorator';
import { AuthModule } from '@/store/modules/authmanager';
import { ISignUpRequest, IAuthStatus } from '@/shared';
import VueRouter from 'vue-router';
import { ValidationProvider, ValidationObserver } from 'vee-validate';
import { SecurityMixin } from '@/components/security/SecurityMixin';
import { Mixins } from 'vue-mixin-decorator';
import ValidatingInput from '@/components/forms/ValidatingInput.vue';
import { ProviderInstance, ValidationResult } from 'vee-validate/dist/types/types';

@Component({
  name: 'SignUp',
  components: { ValidationProvider, ValidationObserver, ValidatingInput }
})
export default class SignUp extends Mixins<SecurityMixin>(SecurityMixin) {
  public userName = '';

  public termsStatus = 'not_accepted';

  public submitted = false;

  protected passwordErrorText = '';

  protected isPasswordConfirmed: boolean | null = null;

  public async handleSubmit(evt: Event): Promise<void> {
    const creds: ISignUpRequest = {
      name: this.userName,
      password: this.password,
      acceptedTerms: this.termsStatus === 'accepted'
    };
    evt.preventDefault();
    this.submitted = true;

    if (creds.name && creds.password && creds.acceptedTerms) {
      await AuthModule.signUpAsync(creds);

      if (!this.loginStatus.isError) {
        const router: VueRouter = this.$router as VueRouter;
        router.push({ path: '/confirmuser', query: { userName: this.userName } });
      }
    }
  }

  protected async validatePassword(passType: number): Promise<void> {
    let curControl: ProviderInstance | null = null;

    switch (passType) {
      case 1:
        curControl = this.$refs.valPasswordProvider as ProviderInstance;
        break;
      case 2:
        curControl = this.$refs.valConfirmPasswordProvider as ProviderInstance;
        break;
    }

    if (curControl !== null) {
      const valResult: ValidationResult = await curControl.validate();

      let hasErrors = false;

      if (valResult?.errors) {
        if (valResult.errors.length > 0) {
          this.passwordErrorText = valResult.errors[0];
          hasErrors = true;
        }
      }
      this.isPasswordConfirmed = !hasErrors;
    }
  }

  protected get loginStatus(): IAuthStatus {
    return AuthModule.status;
  }

  protected get currentError(): string {
    return AuthModule.errorMessage;
  }

  protected mounted() {
    if (AuthModule.credentials?.name) {
      this.userName = AuthModule.credentials.name;
    }
  }
}
</script>

<style scoped lang="scss">
@import '@/design/userforms.scss';
</style>

import { Mixin } from 'vue-mixin-decorator';
import { rules } from '@/shared/validators/index';
import Vue from 'vue';

enum InputType {
  UserName = 1,
  Password = 2,
  PasswordConfirmation = 3
}

@Mixin
export class SecurityMixin extends Vue {
  public displayUserError: boolean | null = null;

  public displayPasswordError: boolean | null = null;

  public displayPasswordConfirmationError: boolean | null = null;

  public password = '';

  public confirmPassword = '';

  public get getDisplayPasswordConfirmationError(): null | boolean {
    let isValid: null | boolean = null;

    if (this.displayPasswordConfirmationError === null && this.displayPasswordError !== null) {
      console.log(`displayPasswordError ${this.displayPasswordError}`);
      isValid = !this.displayPasswordError;
    }

    if (this.displayPasswordConfirmationError !== null && this.displayPasswordError === null) {
      console.log(`displayPasswordConfirmationError ${this.displayPasswordConfirmationError}`);
      isValid = !this.displayPasswordConfirmationError;
    }

    if (this.displayPasswordConfirmationError === null && this.displayPasswordError === null) {
      console.log(`displayPasswordError and displayPasswordConfirmationError are null`);
      isValid = null;
    } else {
      console.log(
        `displayPasswordConfirmationError ${this.displayPasswordConfirmationError} displayPasswordError ${this.displayPasswordError}`
      );
      isValid = !(this.displayPasswordConfirmationError || this.displayPasswordError);
    }

    console.log(
      `isValid ${isValid} displayPasswordConfirmationError ${this.displayPasswordConfirmationError} displayPasswordError ${this.displayPasswordError}`
    );

    return isValid;
  }

  public get emailRules() {
    return rules.email();
  }

  public get requiredRule() {
    return rules.required();
  }

  public passwordConfirmRule(targetControl: string) {
    return rules.passwordConfirm(targetControl);
  }

  public getValStatus(errArray: string[] | undefined, inputType: InputType): void {
    let hasErrs: boolean | null = null;

    if (errArray) {
      hasErrs = errArray.length > 0;
    }

    console.log(`inputType ${inputType} hasErrs ${hasErrs}`);
    if (hasErrs) console.log(errArray);

    if (hasErrs !== null) {
      switch (inputType) {
        case InputType.UserName:
          this.displayUserError = hasErrs;
          break;
        case InputType.Password:
          this.displayPasswordError = hasErrs;
          break;
        case InputType.PasswordConfirmation:
          this.displayPasswordConfirmationError = hasErrs;
          break;
      }
    }
  }
}

import { extend } from 'vee-validate';
import { required, email } from 'vee-validate/dist/rules';

const confirmTarget = 'target';

extend('email', {
  ...email,
  message: 'Must be in the form of an email'
});

// Override the default message.
extend('required', {
  ...required,
  message: '{_field_} is required'
});

extend('verify_password', {
  message: `The password must contain at least: 1 uppercase letter, 1 lowercase letter, 1 number, and one special character (E.g. , . _ & ? etc)`,
  validate: (value) => {
    const strongRegex = new RegExp('^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*])(?=.{8,})');
    return strongRegex.test(value);
  }
});

extend('password_confirmation', {
  message: 'Password confirmation does not match',
  params: [confirmTarget],
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  validate(value, params: Record<string, any>) {
    const isValid = value === params[confirmTarget];
    // logger.log(`validated: ${value} = ${params[confirmTarget]}: ${isValid}`);
    return isValid;
  }
});

class SecurityRules {
  public required() {
    return {
      required: true
    };
  }

  public passwordConfirm(targetControl: string) {
    // logger.log(`target control: ${targetControl}`);
    return {
      // eslint-disable-next-line @typescript-eslint/camelcase
      verify_password: true,
      // eslint-disable-next-line @typescript-eslint/camelcase
      password_confirmation: { target: targetControl },

      required: true
    };
  }

  public email() {
    return {
      required: true,
      email: true
    };
  }
}

export const rules = new SecurityRules();

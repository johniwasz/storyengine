<template>
  <ValidationObserver
    id="valinputobserver"
    v-bind:value="currentValue"
    v-on:input="$emit('input', $event.target.value)"
  >
    <ValidationProvider :name="fieldName" :rules="rules" ref="valProvider">
      <b-form-group>
        <div class="input-group">
          <b-icon v-if="displayIcon" :icon="displayIcon" class="align-self-center"></b-icon>

          <b-form-input
            ref="formInput"
            align-h="start"
            :id="name"
            v-model="currentValue"
            :fieldName="fieldName"
            :placeholder="placeholder"
            @change="applyValidation()"
            autocomplete="on"
            :type="type"
            :lazy="true"
            :state="validationStatus"
            :readonly="isReadOnly"
          ></b-form-input>
        </div>
        <b-form-invalid-feedback :state="validationStatus">{{ errors[0] }}</b-form-invalid-feedback>
      </b-form-group>
    </ValidationProvider>
  </ValidationObserver>
</template>

<script lang="ts">
import { Component, Prop, Emit, Ref, Vue } from 'vue-property-decorator';
import { ValidationProvider, ValidationObserver } from 'vee-validate';
import { ProviderInstance, ValidationResult } from 'vee-validate/dist/types/types';
import { BFormInput } from 'bootstrap-vue';

@Component({
  name: 'sb-validating-input',
  inheritAttrs: false,
  components: { ValidationProvider, ValidationObserver, BFormInput }
})
export default class ValidatingInput extends Vue {
  public displayErrors: boolean | null = null;

  @Prop()
  public displayIcon!: string;

  @Prop()
  public placeholder!: string;

  @Prop()
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  public rules!: any;

  // @PropSync('value', { type: String, required: true })
  // public syncedValue!: string;

  @Prop({ type: String, required: true })
  public value!: string;

  public errors: string[] = [];

  @Prop({ required: true })
  public fieldName!: string;

  @Prop({ required: true })
  public name!: string;

  @Prop()
  public readonly!: boolean;

  @Prop({
    default: 'text',
    type: String,
    validator: (value: string) => {
      return (
        ['text', 'number', 'email', 'password', 'search', 'url', 'tel', 'date', 'time', 'range', 'color'].indexOf(
          value
        ) !== -1
      );
    }
  })
  public type!: string;

  protected syncedVal: string = this.value;

  @Ref()
  protected formInput!: BFormInput;

  @Emit()
  public validated(isValid: boolean | null): void {
    this.displayErrors = isValid === null ? null : !isValid;
  }

  protected get validationStatus(): null | boolean {
    if (this.displayErrors === null) {
      return null;
    }

    return !this.displayErrors;
  }

  protected async applyValidation(): Promise<void> {
    let hasErrs: boolean | null = null;

    this.errors = [];

    const valInstance: ProviderInstance = this.$refs.valProvider as ProviderInstance;

    const valResult: ValidationResult = await valInstance.validate();

    if (valResult?.errors) {
      hasErrs = valResult.errors.length > 0;
      this.errors = valResult.errors;
    }

    if (hasErrs !== null) {
      this.validated(!hasErrs);
    }
  }

  protected get isReadOnly() {
    return this.readonly;
  }

  protected get currentValue() {
    return this.syncedVal;
  }
  protected set currentValue(curVal: string) {
    this.syncedVal = curVal;
  }

  protected async mounted() {
    if (this.currentValue) {
      const valInstance: ProviderInstance = this.$refs.valProvider as ProviderInstance;

      if (valInstance) {
        const valResult = await valInstance.validate();
        this.validated(valResult.valid);
      }
    }
  }
}
</script>

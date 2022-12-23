<template>
  <b-modal id="newDeploymentModal" ref="newDeploymentModal" title="New Deployment" @ok="handleOk">
    <form ref="newDeploymentForm" @submit.stop.prevent>
      <b-form-group
        id="fieldset-1"
        label="Client Type:"
        label-for="clientTypeSelect"
        label-cols-sm="4"
        label-cols-lg="3"
        align-h="start"
      >
        <b-form-select
          id="clientTypeSelect"
          v-model="newDeployment.clientType"
          :options="botClientOptions"
          :state="isClientTypeValid"
        ></b-form-select>
        <b-form-invalid-feedback :state="isClientTypeValid">
          Please select a client type.
        </b-form-invalid-feedback>
      </b-form-group>

      <b-form-group
        id="fieldset-2"
        :description="guidanceText"
        label="Client Identifier:"
        label-for="clientIdInput"
        label-cols-sm="4"
        label-cols-lg="3"
        align-h="start"
      >
        <b-form-input
          id="clientIdInput"
          v-model="newDeployment.clientId"
          aria-describedby="input-live-help input-live-feedback"
          :placeholder="placeholderText"
          trim
          :state="isClientIdValid"
        ></b-form-input>
        <b-form-invalid-feedback :state="isClientIdValid">
          Your user ID must be 5-12 characters long.
        </b-form-invalid-feedback>
        <b-form-valid-feedback :state="isClientIdValid">
          Looks Good.
        </b-form-valid-feedback>
      </b-form-group>
      <b-form-group
        id="fieldset-3"
        label="Alias:"
        label-for="aliasInput"
        label-cols-sm="4"
        label-cols-lg="3"
        align-h="start"
      >
        <b-form-input
          id="aliasInput"
          v-model="newDeployment.alias"
          aria-describedby="input-live-help input-live-feedback"
          placeholder="LIVE"
          trim
        ></b-form-input>
      </b-form-group>
    </form>
  </b-modal>
</template>

<script lang="ts">
// tslint:disable-next-line
// import { VersionModule } from '@/store/modules/versionmanager';
import { DeploymentModule } from '@/store/modules/deploymentmanager';
import { BotClientOptions, IAddDeploymentRequest } from '@/shared';
import { Component, Ref, Vue } from 'vue-property-decorator';
import { BForm, BModal, BvEvent } from 'bootstrap-vue';

@Component({ name: 'NewDeployment' })
export default class NewDeployment extends Vue {
  // tslint:disable-next-line
  private projectLabel = '';

  // tslint:disable-next-line
  private newDeploymentInternal: IAddDeploymentRequest = {
    clientType: 'unknown',
    projectId: '',
    versionId: '',
    clientId: '',
    alias: 'LIVE'
  };

  @Ref()
  private readonly newDeploymentForm!: BForm;

  @Ref()
  private readonly newDeploymentModal!: BModal;

  get newDeployment(): IAddDeploymentRequest {
    return this.newDeploymentInternal;
  }

  set newDeployment(dep: IAddDeploymentRequest) {
    this.newDeploymentInternal = dep;
  }

  get isClientTypeValid(): boolean {
    const isValid = this.newDeployment.clientType !== 'unknown';

    return isValid;
  }

  get isClientIdValid(): boolean {
    let isValid = this.newDeploymentInternal.clientId ? true : false;

    if (isValid) {
      isValid = this.newDeploymentInternal.clientId.length > 0;
    }

    return isValid;
  }

  get botClientOptions() {
    return BotClientOptions;
  }

  get placeholderText(): string {
    let retText = '';

    if (this.newDeployment) {
      switch (this.newDeployment.clientType) {
        case 'unknown':
          retText = '';
          break;
        case 'sms':
          retText = '+15551212';
          break;
        case 'alexa':
          retText = 'amzn1.ask.skill.11111111-2222-3333-4444-123456789012';
          break;
        case 'googleHome':
          retText = 'google-project-name';
          break;
      }
    }

    return retText;
  }

  get guidanceText(): string {
    let retText = '';

    if (this.newDeployment) {
      switch (this.newDeployment.clientType) {
        case 'unknown':
          retText = 'Please select a valid client type';
          break;
        case 'sms':
          retText =
            'Enter the phone number that will route to this deployment. Use the interational prefix (+1 for US)';
          break;
        case 'alexa':
          retText = 'This Alexa Skill id';
          break;
        case 'googleHome':
          retText = 'Google Action project id';
          break;
      }
    }

    return retText;
  }

  public handleOk(bvModalEvt: BvEvent): void {
    // Prevent modal from closing
    bvModalEvt.preventDefault();
    // Trigger submit handler
    this.handleSubmit();
  }

  public async handleSubmit(): Promise<void> {
    // Exit when the form isn't valid
    if (!this.checkFormValidity()) {
      return;
    } else {
      await DeploymentModule.addDeploymentAsync(this.newDeployment);
    }

    // Hide the modal manually
    this.$nextTick(() => {
      this.newDeploymentModal.hide();
    });
  }

  public show(projectId: string, versionId: string): void {
    // Show the modal form.
    this.newDeploymentInternal = this.initializeDeployment(projectId, versionId);
    this.newDeploymentModal.show();
  }

  private checkFormValidity(): boolean {
    const valid = this.newDeploymentForm.checkValidity();
    // this.nameState = valid;
    return valid;
  }

  private initializeDeployment(projectId: string, versionId: string): IAddDeploymentRequest {
    const retDeployment: IAddDeploymentRequest = {
      clientType: 'unknown',
      projectId,
      versionId,
      clientId: '',
      alias: 'LIVE'
    };
    return retDeployment;
  }
}
</script>

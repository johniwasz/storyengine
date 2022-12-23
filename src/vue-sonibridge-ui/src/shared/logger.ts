import { format } from 'date-fns';

import { displayTimeFormat } from './constants';

export const logger = {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  info(message: string, data: any): void {
    console.log(`Log ${format(Date.now(), displayTimeFormat)}: ${message}`);
    if (data) {
      console.log(JSON.stringify(data, null, 2));
    }
  },

  log(message: string): void {
    this.info(message, undefined);
  }
};

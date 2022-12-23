#!/bin/bash

source /myagent/env.sh

trap 'cleanup; lsexit 130' INT
trap 'cleanup; exit 143' TERM

/myagent/config.sh --unattended \
  --agent "${AZP_AGENT_NAME:-$(hostname)}" \
  --url $AZP_URL \
  --auth PAT \
  --token $AZP_TOKEN \
  --pool "$AZP_POOL" \
  --work "$AZP_WORK" \
  --replace \
  --acceptTeeEula

exec /myagent/externals/node/bin/node /myagent/bin/AgentService.js interactive
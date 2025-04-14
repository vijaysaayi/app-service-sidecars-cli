# AppService Sidecars CLI

## Overview

The AppService Sidecars CLI is a tool designed to manage sidecar configurations and operations for AppService environments. It provides commands to bring up sidecars, bring them down, and view logs.

## Commands

### 1. `up`

The `up` command is used to start the sidecars defined in the `sidecars.yaml` configuration file.

```bash
AppServiceSidecarsCli up
```

- Reads the `sidecars.yaml` file located in the `sample/` directory.
- Starts the sidecars using Docker.

### 2. `down`

The `down` command stops and removes the sidecars that were started using the `up` command.

```bash
AppServiceSidecarsCli down
```

- Stops and removes the Docker containers associated with the sidecars.

### 3. `logs`

The `logs` command fetches and displays logs from the running sidecars.

```bash
AppServiceSidecarsCli logs
```

- Retrieves logs from the Docker containers associated with the sidecars.

## Configuration

The CLI uses a `sidecars.yaml` file to define the sidecar configurations. Ensure that this file is present in the `sample/` directory and is correctly configured before running the commands.

### Example `sidecars.yaml`:

```yaml
version: v1

appsettings:
  - name: "WEBUI_AUTH"
    value: "false"
  
  - name: "OPENAI_API_BASE_URL"
    value: "http://localhost:11434/v1"

  - name: "SLM_PORT"
    value: "11434"

containers:
  # by default all environment variables are passed to main container.
  - name: "main"
    image: "ghcr.io/open-webui/open-webui:main"
    targetPort: "8080"
    isMain: true
    authType: Anonymous

  - name: "slm"
    image: "demoacr.azurecr.io/phi-4-mini-instruct-q4_0:v6"
    targetPort: "11434"
    isMain: false
    authType: UserCredential
    userName: ${ACR_USERNAME}
    passwordSecret: ${ACR_PASSWORD}
    environmentVariables:
      - name: "SLM_PORT"
        value: "SLM_PORT"
```

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request.

## License

This project is licensed under the MIT License.

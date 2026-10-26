# Aerostar Mission Planner - signing and portal publishing

Every push to the `aerostar` branch of AerostarDynamics/MissionPlanner-Aerostar (workflow `.github/workflows/main.yml`,
job "Build Release"):

1. builds the MSI `AerostarMissionPlanner-<build>.msi`;
2. **signs it** as "Aerostar Dynamics Inc." - EV certificate `aerostar-dynamics-inc.crt` (public part only; the private
   key never leaves Google Cloud KMS, project windows-code-signing), through the Google KMS CNG provider and signtool,
   logged in keyless with workload identity. Same steps as AerostarDynamics/Sign-Windows-Install's sign-file action;
3. attaches the signed MSI + .sha256 to the GitHub release `aerostar-latest`;
4. **publishes it to the customer portal as the new beta** - only when `portal-version.txt` holds a version that has not
   been published yet (no git tag `portal-<version>`): uploads `MissionPlanner-<version>.msi/.sha256/.json` to the bucket
   aerostar-mission-planner-install (project gcp-operations-451421) and registers the version with the portal back end.
   The back end requires the version format `1.3.83.01` and the file name `MissionPlanner-<version>.msi`.

To put a new build on the portal: raise the last number in `portal-version.txt` (e.g. 1.3.83.01 -> 1.3.83.02) in the
same push. When the beta has been checked, promote it with **Actions -> Aerostar MSI GA -> Run workflow** (branch aerostar).

Variables used (no secrets): repository - MSI_DEPLOYMENT_*, WINDOWS_SIGNING_WORKLOAD_IDENTITY_PROVIDER; organization
(shared with this repository) - WINDOWS_SIGNING_PROJECT_ID, WINDOWS_SIGNING_SERVICE_ACCOUNT, CODE_SIGNING_*, KMS_CNG_*.
Google Cloud only lets the `aerostar` branch of this repository in (workload identity provider attribute conditions).

Scripts: `add-signtool.ps1` (finds signtool, Dave Howie 2025), `create-portal-files.py` (portal file names + .json).

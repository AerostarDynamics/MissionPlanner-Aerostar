import argparse
import hashlib
import json
import os
import re
import shutil
import subprocess
import time

# Copyright 2025-2026 AeroStar Dynamics
#
# Prepares the files the Aerostar customer portal expects in the Google Cloud Storage bucket for one Mission Planner
# version: MissionPlanner-<portal version>.msi (the portal back end builds this exact file name from the version),
# a matching .sha256 and a .json recording what was built. Based on Dave Howie's create-mission-planner-json.py.

# Same rule as the portal back end (gcp-operations-backend mp_service.py), e.g. 1.3.83.01.
_portal_version_regex = r'^\d{1,2}\.\d{1,2}\.\d{2}\.\d{2}$'


def _short_sha(repo_path: str) -> str:
    result = subprocess.run(["git", "rev-parse", "--short=8", "HEAD"], cwd=repo_path, capture_output=True, text=True)
    return result.stdout.strip()


def _sha256(path: str) -> str:
    digest = hashlib.sha256()
    with open(path, "rb") as f:
        for block in iter(lambda: f.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def main(args):
    version = args.portal_version.strip()
    if re.match(_portal_version_regex, version) is None:
        raise SystemExit(f"Portal version '{version}' must look like 1.3.83.01 (portal back end rule).")

    os.makedirs(args.output_path, exist_ok=True)
    base_name = f"MissionPlanner-{version}"
    msi_out = os.path.join(args.output_path, f"{base_name}.msi")
    shutil.copyfile(args.msi, msi_out)

    msi_sha = _sha256(msi_out)
    with open(os.path.join(args.output_path, f"{base_name}.sha256"), "w", newline="\n") as f:
        f.write(f"{msi_sha}  {base_name}.msi\n")

    output_json = {
        "version": version,
        "timestamp": time.strftime("%Y-%m-%d %H.%M.%S %z", time.localtime()),
        "product": "Aerostar Mission Planner",
        "build-version": args.build_version,
        "built-msi-name": os.path.basename(args.msi),
        "msi-sha256": msi_sha,
        "mission-planner-repo": args.repository,
        "mission-planner-ref": args.ref,
        "mission-planner-hash": _short_sha(args.repo_path),
    }
    with open(os.path.join(args.output_path, f"{base_name}.json"), "w") as f:
        json.dump(output_json, f, indent=4)

    print(json.dumps(output_json, indent=4))


# Example:
#   python create-portal-files.py --portal-version 1.3.83.01 --msi Msi/AerostarMissionPlanner-1.3.9795.5838.msi \
#   --build-version 1.3.9795.5838 --repository AerostarDynamics/MissionPlanner-Aerostar --ref aerostar --output-path portal
if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Prepare the portal files for one Aerostar Mission Planner MSI.")
    parser.add_argument("--portal-version", required=True, help="Version shown on the portal, e.g. 1.3.83.01.")
    parser.add_argument("--msi", required=True, help="The signed MSI built by the workflow.")
    parser.add_argument("--build-version", required=True, help="Mission Planner build version inside the MSI.")
    parser.add_argument("--repository", required=True, help="owner/repo that built it.")
    parser.add_argument("--ref", required=True, help="Branch that built it.")
    parser.add_argument("--repo-path", default=".", help="Path to the git checkout.")
    parser.add_argument("--output-path", required=True, help="Folder for the three portal files.")
    main(parser.parse_args())

#!/usr/bin/env bash
set -euo pipefail

if [[ $# -ne 1 ]]; then
  echo "Usage: $0 <package.nupkg>" >&2
  exit 2
fi

package_path="$1"

if [[ -z "$package_path" || ! -f "$package_path" ]]; then
  echo "Package not found: $package_path" >&2
  exit 2
fi

if ! command -v unzip >/dev/null 2>&1; then
  echo "unzip is required to verify package contents." >&2
  exit 2
fi

package_entries="$(unzip -Z1 "$package_path")"
target_frameworks=(
  "net10.0-android"
  "net10.0-ios"
  "net10.0-maccatalyst"
)
assembly="StateManagementSharp.Maui.dll"

problem_count=0

for target_framework in "${target_frameworks[@]}"; do
  if ! grep -Eq "^lib/${target_framework}[^/]*/${assembly}$" <<<"$package_entries"; then
    echo "Missing lib/${target_framework}*/${assembly}" >&2
    problem_count=$((problem_count + 1))
  fi
done

# The MAUI package must NOT ship a netstandard2.0 asset (that lives in the core package).
if grep -Eq "^lib/netstandard2\.0" <<<"$package_entries"; then
  echo "Unexpected netstandard2.0 assets in the MAUI package." >&2
  problem_count=$((problem_count + 1))
fi

# Fody is a compile-time weaver: no Fody/PropertyChanged assemblies may leak into lib/.
if grep -Eiq "^lib/.*/(Fody|PropertyChanged)\.dll$" <<<"$package_entries"; then
  echo "Fody/PropertyChanged weaver assemblies leaked into the package lib/." >&2
  problem_count=$((problem_count + 1))
fi

# The dedicated MAUI README must be packed (not the core/root README).
readme_content="$(unzip -p "$package_path" README.md 2>/dev/null || true)"
if ! grep -Eq "^README\.md$" <<<"$package_entries"; then
  echo "README.md is missing from the package." >&2
  problem_count=$((problem_count + 1))
elif ! grep -Eq "^# StateManagementSharp\.Maui\b" <<<"$readme_content"; then
  echo "Packed README.md is not the dedicated MAUI README (expected a '# StateManagementSharp.Maui' heading)." >&2
  problem_count=$((problem_count + 1))
fi

if [[ "$problem_count" -gt 0 ]]; then
  echo "MAUI package verification failed: $problem_count problem(s)." >&2
  exit 1
fi

echo "MAUI package verification passed: $package_path"

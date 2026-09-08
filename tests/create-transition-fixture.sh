#!/bin/sh
set -eu

# Every run owns a new repository; interactive demos and user repos are untouched.
fixture_dir=$(mktemp -d build/atlas-transitions-XXXXXX)
git init -q "$fixture_dir"
cp tests/fixtures/transitions-before.coil "$fixture_dir/domain.coil"
git -C "$fixture_dir" add domain.coil
git -C "$fixture_dir" -c user.name='Atlas Test' -c user.email='atlas-test@example.invalid' \
    commit -qm 'Establish structures before transition'
cp tests/fixtures/transitions-after.coil "$fixture_dir/domain.coil"
git -C "$fixture_dir" add domain.coil
git -C "$fixture_dir" -c user.name='Atlas Test' -c user.email='atlas-test@example.invalid' \
    commit -qm 'Modify, construct, and remove every glyph family'
printf '%s\n' "$fixture_dir"

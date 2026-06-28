#!/usr/bin/env bash
set -euo pipefail

# add-module.sh — Scaffold a new business module from the Catalog pattern.
# Usage: ./scripts/add-module.sh Orders

MODULE_NAME="${1:-}"
if [ -z "$MODULE_NAME" ]; then
    echo "Usage: ./scripts/add-module.sh <ModuleName>"
    echo "Example: ./scripts/add-module.sh Orders"
    exit 1
fi

# Validate PascalCase
if echo "$MODULE_NAME" | grep -qE '[^a-zA-Z0-9]|[0-9]'; then
    if echo "$MODULE_NAME" | grep -q '[^a-zA-Z0-9]'; then
        echo "Error: Module name must be PascalCase (letters and numbers only, no spaces or special characters)."
        exit 1
    fi
fi

SLN_DIR="src/Backend"
MODULES_DIR="$SLN_DIR/Modules"
SLN_FILE=$(ls "$SLN_DIR"/*.slnx 2>/dev/null | head -1)

if [ ! -f "$SLN_FILE" ]; then
    echo "Error: Solution file not found in $SLN_DIR/"
    exit 1
fi

echo "Scaffolding module: $MODULE_NAME"

# Copy Catalog as template
for LAYER in Domain Application Infrastructure; do
    SRC="$MODULES_DIR/Catalog/Catalog.$LAYER"
    DST="$MODULES_DIR/$MODULE_NAME/$MODULE_NAME.$LAYER"

    mkdir -p "$DST"

    if [ -f "$SRC/Catalog.$LAYER.csproj" ]; then
        sed "s/Catalog/$MODULE_NAME/g" "$SRC/Catalog.$LAYER.csproj" > "$DST/$MODULE_NAME.$LAYER.csproj"
    fi

    echo "  Created $MODULE_NAME.$LAYER"
done

# Add to solution file (insert before ApiGateway)
awk -v mod="$MODULE_NAME" '
  /<Project Path="ApiGateway/ {
    print "  <Project Path=\"Modules/" mod "/" mod ".Infrastructure/" mod ".Infrastructure.csproj\" />"
    print "  <Project Path=\"Modules/" mod "/" mod ".Application/" mod ".Application.csproj\" />"
    print "  <Project Path=\"Modules/" mod "/" mod ".Domain/" mod ".Domain.csproj\" />"
  }
  { print }
' "$SLN_FILE" > "$SLN_FILE.tmp" && mv "$SLN_FILE.tmp" "$SLN_FILE"

# Add reference to Host.Api.csproj (insert before closing </ItemGroup>)
HOST_CSPROJ="$SLN_DIR/Host.Api/Host.Api.csproj"
if [ -f "$HOST_CSPROJ" ] && ! grep -q "$MODULE_NAME.Infrastructure" "$HOST_CSPROJ"; then
    # Insert before the ProjectReference </ItemGroup> (after Catalog.Infrastructure line)
    awk -v mod="$MODULE_NAME" '
      /Catalog.Infrastructure/ { print; print "    <ProjectReference Include=\"..\\Modules\\" mod "\\" mod ".Infrastructure\\" mod ".Infrastructure.csproj\" />"; next }
      { print }
    ' "$HOST_CSPROJ" > "$HOST_CSPROJ.tmp" && mv "$HOST_CSPROJ.tmp" "$HOST_CSPROJ"
    echo "  Added reference to Host.Api.csproj"
fi

echo ""
echo "Module '$MODULE_NAME' scaffolded."
echo ""
echo "Next steps:"
echo "1. Add Domain entities and value objects in $MODULES_DIR/$MODULE_NAME/$MODULE_NAME.Domain/"
echo "2. Add Commands, Queries, Handlers in $MODULES_DIR/$MODULE_NAME/$MODULE_NAME.Application/"
echo "3. Add DbContext, Endpoints, DI in $MODULES_DIR/$MODULE_NAME/$MODULE_NAME.Infrastructure/"
echo "4. Register in Host.Api Program.cs:"
echo "   builder.Services.Add${MODULE_NAME}Module(builder.Configuration);"
echo "   app.Map${MODULE_NAME}Endpoints();"
echo "5. dotnet build $SLN_FILE"

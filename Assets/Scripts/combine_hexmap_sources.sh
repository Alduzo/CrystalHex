#!/bin/bash

# Archivo de salida
output_file="CrystalHex_Reference.md"

# Limpia archivo si ya existe
echo "# HexMap – Código fuente consolidado" > "$output_file"
echo "" >> "$output_file"
echo "_Generado el $(date)_\n" >> "$output_file"

# Recorre todos los archivos .cs (excepto .meta)
for file in *.cs; do
    # Evita procesar este script si está en la misma carpeta
    if [[ "$file" == "${0##*/}" ]]; then
        continue
    fi

    echo "Procesando $file..."

    echo -e "\n---\n" >> "$output_file"
    echo "## 📁 $file" >> "$output_file"
    echo '```csharp' >> "$output_file"
    cat "$file" >> "$output_file"
    echo '```' >> "$output_file"
done

echo -e "\n✅ Consolidación completada en $output_file"

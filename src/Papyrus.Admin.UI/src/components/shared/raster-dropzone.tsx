import { useRef, useState } from "react";
import { ImageSquare as ImagePlus } from "@phosphor-icons/react";
export function RasterDropzone({ onFile }: { onFile: (file: File) => void }) {
  const input = useRef<HTMLInputElement>(null);
  const [error, setError] = useState("");
  const accept = (file?: File) => {
    if (!file) return;
    if (
      !["image/jpeg", "image/png", "image/webp", "image/gif"].includes(
        file.type,
      ) ||
      file.size > 5 * 1024 * 1024
    ) {
      setError("Use JPEG, PNG, WebP or GIF files up to 5 MB.");
      return;
    }
    setError("");
    onFile(file);
  };
  return (
    <div
      onDragOver={(e) => e.preventDefault()}
      onDrop={(e) => {
        e.preventDefault();
        accept(e.dataTransfer.files[0]);
      }}
      onClick={() => input.current?.click()}
      className="cursor-pointer rounded-none border-2 border-dashed border-primary/50 p-8 text-center transition hover:border-primary"
    >
      <input
        ref={input}
        type="file"
        hidden
        accept="image/jpeg,image/png,image/webp,image/gif"
        onChange={(e) => accept(e.target.files?.[0])}
      />
      <ImagePlus className="mx-auto text-primary" weight="regular" />
      <p className="mt-2 text-sm font-medium">
        Drop a raster image here or browse
      </p>
      {error && <p className="mt-2 text-xs text-destructive">{error}</p>}
    </div>
  );
}

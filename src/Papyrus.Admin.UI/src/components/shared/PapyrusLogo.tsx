import type { SVGProps } from "react";

export function PapyrusLogo(props: SVGProps<SVGSVGElement>) {
  return (
    <svg
      viewBox="0 0 100 100"
      fill="currentColor"
      aria-hidden="true"
      {...props}
    >
      <path d="M50 4 62 24l22-11-3 25 17 12-17 12 3 25-22-11-12 20-12-20-22 11 3-25L2 50l17-12-3-25 22 11L50 4Zm0 18-7 12-13-6 2 15-10 7 10 7-2 15 13-6 7 12 7-12 13 6-2-15 10-7-10-7 2-15-13 6-7-12Zm0 18a10 10 0 1 1 0 20 10 10 0 0 1 0-20Z" />
    </svg>
  );
}

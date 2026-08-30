import {
  FormProvider,
  useFormContext,
  Controller,
  type ControllerProps,
  type FieldPath,
  type FieldValues,
} from "react-hook-form";
import { Label } from "./label";
import { cn } from "@/lib/utils";

export const Form = FormProvider;
export function FormField<
  TFieldValues extends FieldValues,
  TName extends FieldPath<TFieldValues>,
>(props: ControllerProps<TFieldValues, TName>) {
  return <Controller {...props} />;
}
export function FormItem({ className, ...props }: React.ComponentProps<"div">) {
  return <div className={cn("flex flex-col gap-2", className)} {...props} />;
}
export function FormLabel({
  className,
  ...props
}: React.ComponentProps<typeof Label>) {
  return <Label className={cn("leading-none", className)} {...props} />;
}
export function FormMessage({
  className,
  ...props
}: React.ComponentProps<"p">) {
  const { formState } = useFormContext();
  return (
    <p
      className={cn("text-sm font-medium text-destructive", className)}
      {...props}
    >
      {props.children ??
        Object.values(formState.errors)[0]?.message?.toString()}
    </p>
  );
}

# ISBN-13 979 Validation Fix

## Issue
The admin resource editor validates ISBN-13 by converting the ISBN-10 into an ISBN-13 and comparing the two values. That logic assumes every ISBN-13 can be derived from an ISBN-10, which is not true for 979-prefix ISBN-13 values. As a result, resources with ISBN-13 values like 9798275082845 raise a validation error when an ISBN-10 is present.

## Fix
The admin validation now treats 979-prefix ISBN-13 values as ISBN-13-only:
- ISBN-10 is ignored and cleared when ISBN-13 starts with 979.
- The ISBN-10 to ISBN-13 comparison is skipped for 979-prefix values.
- The base ISBN selection favors ISBN-13 for these cases.

## Files Updated
- src/R2V2.Web/Areas/Admin/Models/Resource/ResourceAdminService.cs

## Behavior After Fix
- Resources with ISBN-13 starting with 979 can be saved without triggering the ISBN-13 mismatch error.
- ISBN-10 will be cleared so the resource data stays consistent with ISBN standards.

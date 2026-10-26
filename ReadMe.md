# Sesharp

[![Build Status](https://github.com/MrKWatkins/Sesharp/actions/workflows/build.yml/badge.svg)](https://github.com/MrKWatkins/Sesharp/actions/workflows/build.yml)

Generate API Markdown from .NET assemblies and XML documentation. VitePress is
the default output dialect; MkDocs heading syntax is available as an option.

This tool follows the conventions used by my projects and is not intended to be
a general-purpose documentation framework.

## Usage

Build the assemblies in Release configuration with XML documentation and portable
PDBs, keeping dependency assemblies alongside them. Then run:

```bash
sesharp path/to/Library.dll path/to/OtherLibrary.dll \
  --output artifacts/api \
  --repository https://github.com/example/library \
  --source-ref FULL_COMMIT_SHA
```

`--source-ref` selects the Git ref used by source links and defaults to `main`.
Pass the actual source commit for release documentation. PDBs provide source
locations; XML documentation provides summaries, parameters and inherited docs.
Framework inheritance also requires the matching .NET reference-pack XML files.

The output directory is replaced by default. Use a generated staging directory
such as `artifacts/api`; keep handwritten API introductions in the source content
and combine them with generated pages when building the site.

Use `--dialect MkDocs` for the optional MkDocs heading syntax. Both dialects use
consistent IDs for headings and overload links. VitePress pages disable source
edit links and preserve explicit View source links.

## Navigation manifest

Generation writes API and namespace landing pages, type and member pages, and
`navigation.json`. The manifest describes the API independently of any site's
configuration:

```json
{
  "schemaVersion": 1,
  "entries": [
    {
      "id": "N:Example",
      "path": "Example/index.md",
      "title": "Example",
      "kind": "namespace",
      "parent": null
    },
    {
      "id": "T:Example.Thing`1",
      "path": "Example/Thing-T/index.md",
      "title": "Thing<T>",
      "kind": "type",
      "parent": "N:Example"
    },
    {
      "id": "G:Example/Thing-T/Read.md",
      "path": "Example/Thing-T/Read.md",
      "title": "Read",
      "kind": "member",
      "parent": "T:Example.Thing`1"
    }
  ]
}
```

Paths are relative to the generated output root, include `.md`, and use forward
slashes. Titles are plain text, including generic angle brackets; site builders
should treat them as text. Namespace entries have no parent. Nested types point
to their declaring type; members point to their type. Type and individual-member
IDs use XML documentation identities; overload-group IDs use `G:` plus their page
path. Each overload group has one page with linked anchors for its overloads.
The root `index.md` is an API landing page and is not a manifest entry.

Entries are ordered by path and ID using ordinal comparison. Namespaces shared
by multiple assemblies have one landing page. Conflicting type output paths
fail generation rather than overwriting another assembly's API.

Build sidebars from namespace/type entries; keep member lists on type pages.
Consumers should check `schemaVersion` before reading the manifest.

## Validation

The normal build runs unit tests and builds the complete test API with VitePress
1.6.4 in a Node 24 Docker image. Tests cover rendering, literal text, nested types,
inherited framework documentation and internal anchors. VitePress dead-link
checking stays enabled. Docker must be available when running the test suite.

## Name

A combination of [Seshat](https://en.wikipedia.org/wiki/Seshat), the Egyptian
goddess of writing, wisdom and knowledge, and C Sharp. Yeah, it's not great, but
all the various permutations of 'documentation generator' are gone and naming
things is hard.

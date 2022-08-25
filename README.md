# CliStore

The goal is to demonstrate how to implement persistent parameters. Kinda what *Azure CLI* and *AWS CLI* do to improve developer experience.

For example, Azure CLI offers persisted parameters that enable you to store parameter values for continued use.

```bash
# oleksii_nikiforov in ~/projects
> az config -h

Group
    az config : Manage Azure CLI configuration.
        Available since Azure CLI 2.10.0.
        WARNING: This command group is experimental and under development. Reference and support
        levels: https://aka.ms/CLI_refstatus

Subgroups:
    param-persist : Manage parameter persistence.

Commands:
    get           : Get a configuration.
    set           : Set a configuration.
    unset         : Unset a configuration.
```

## Idea

The general approach is based on idea of storing configuration in well-known location.

For example: 

- `~/.azure/config` is used by Azure CLI
- `~/.aws/config` and `~/.aws/credentials` are used by AWS CLI

The solution is not intended to be used as-is in production but is a good starting point.

## Tests

This projects utilized snapshot testing via [Verify](https://github.com/VerifyTests/Verify).
For more details regarding the implementation see [./tests/CliStore.Tests](./tests/CliStore.Tests)

#!/usr/bin/env python3
"""Compatibility wrapper for the current generated model pack."""

from pathlib import Path
import runpy


runpy.run_path(str(Path(__file__).with_name("generate_fourfold_model_pack.py")), run_name="__main__")

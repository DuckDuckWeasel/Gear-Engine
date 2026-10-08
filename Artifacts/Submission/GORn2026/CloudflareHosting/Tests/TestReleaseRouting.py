"""Regression coverage for the public landing and playable release layout."""

import contextlib
import importlib.util
import io
import json
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

SCRIPT = Path(__file__).resolve().parents[1] / "PrepareRelease.py"
spec = importlib.util.spec_from_file_location("prepare_release", SCRIPT)
prepare = importlib.util.module_from_spec(spec)
spec.loader.exec_module(prepare)


class ReleaseRoutingTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name)
        self.source = self.root / "Source"
        self.deploy = self.root / "Deploy"
        self.canonical = self.deploy / "games" / "gear-engine"
        self.source.mkdir()
        (self.source / "index.html").write_text('<title>Playable game</title>')
        (self.source / "Build").mkdir()
        (self.source / "Build" / "Game.loader.js").write_text('loader')
        (self.source / "StreamingAssets").mkdir()
        (self.source / "StreamingAssets" / "settings.json").write_text('{}')
        press_kit = self.source / "PressKit"
        press_kit.mkdir()
        (press_kit / "index.html").write_text(
            '<title>Landing page</title><a href="#game">Game</a>'
            '<a class="play-link" href="/games/gear-engine/play">Play</a>'
            '<img src="Media/Logo.png"><video poster="Media/Logo.png"></video>'
            '<style>src:url("Fonts/Font.woff2")</style>'
            '<a href="GearEnginePublicPressKit.zip">Download</a>'
        )
        (press_kit / "Media").mkdir()
        (press_kit / "Media" / "Logo.png").write_bytes(b'logo')
        for name, value in {
            "SOURCE_DIR": self.source, "DEPLOY_DIR": self.deploy,
            "CANONICAL_DIR": self.canonical,
        }.items():
            p = patch.object(prepare, name, value)
            p.start()
            self.addCleanup(p.stop)
        with contextlib.redirect_stdout(io.StringIO()):
            prepare.main()

    def test_landing_and_player_are_separate(self):
        self.assertIn('Landing page', (self.canonical / 'index.html').read_text())
        self.assertIn('Playable game', (self.canonical / 'play/index.html').read_text())
        self.assertTrue((self.canonical / 'play/Build/Game.loader.js').is_file())
        self.assertTrue((self.canonical / 'play/StreamingAssets/settings.json').is_file())
        self.assertFalse((self.canonical / 'play/PressKit').exists())

    def test_landing_resources_and_anchors_keep_working(self):
        landing = (self.canonical / 'index.html').read_text()
        self.assertIn('href="#game"', landing)
        self.assertIn('src="PressKit/Media/Logo.png"', landing)
        self.assertIn('poster="PressKit/Media/Logo.png"', landing)
        self.assertIn('url("PressKit/Fonts/Font.woff2")', landing)
        self.assertIn('href="PressKit/GearEnginePublicPressKit.zip"', landing)
        self.assertIn('href="/games/gear-engine/play"', landing)
        self.assertTrue((self.canonical / 'PressKit/Media/Logo.png').is_file())

    def test_legacy_page_redirects_and_player_headers(self):
        redirects = (self.deploy / '_redirects').read_text()
        self.assertIn('/games/gear-engine/PressKit/ /games/gear-engine/ 301', redirects)
        headers = (self.deploy / '_headers').read_text()
        self.assertIn('/games/gear-engine/play/Build/*.wasm.unityweb\n  Content-Type: application/wasm', headers)
        self.assertIn('/games/gear-engine/play/StreamingAssets/aa/catalog.bin', headers)

    def test_manifest_identifies_each_public_destination(self):
        manifest = json.loads((self.canonical / 'CloudflareReleaseManifest.json').read_text())
        self.assertEqual('https://leonardolycan.com/games/gear-engine/', manifest['landingPageUrl'])
        self.assertEqual('https://leonardolycan.com/games/gear-engine/play', manifest['canonicalUrl'])


if __name__ == '__main__':
    unittest.main()

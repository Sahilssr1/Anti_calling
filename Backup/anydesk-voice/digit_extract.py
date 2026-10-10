"""Extract an AnyDesk ID (9 digits) from a speech transcript.

Handles:
- Arabic digits: 0-9
- Devanagari digits: ०१२३४५६७८९
- English number words: zero/oh, one, two, ... nine (+ double/triple)
- Hindi number words in Roman script: ek, do, teen, char, paanch, chhah, saat, aath, nau, ...
- Hindi number words in Devanagari: एक, दो, तीन, चार, पांच, छह, सात, आठ, नौ, ...
"""

import re
import unicodedata

DEVANAGARI_DIGITS = str.maketrans("०१२३४५६७८९", "0123456789")

NUMBER_WORDS = {
    # English
    "zero": "0", "oh": "0", "o": "0",
    "one": "1", "two": "2", "three": "3", "four": "4", "five": "5",
    "six": "6", "seven": "7", "eight": "8", "nine": "9",
    # Hindi — Roman script, common spelling variants
    "shunya": "0", "shoonya": "0", "sifar": "0", "sifr": "0",
    "jeero": "0", "jiro": "0",
    "ek": "1",
    "do": "2",
    "teen": "3", "tin": "3",
    "char": "4", "chaar": "4",
    "paanch": "5", "panch": "5",
    "chhah": "6", "chhe": "6", "chah": "6",
    "saat": "7", "sath": "7",
    "aath": "8",
    "nau": "9", "nauu": "9",
    # Hindi — Devanagari script (Whisper often outputs this for Hindi speech)
    "शून्य": "0", "जीरो": "0",
    "एक": "1", "दो": "2", "तीन": "3", "चार": "4", "पांच": "5",
    "छह": "6", "छः": "6", "सात": "7", "आठ": "8", "नौ": "9",
}

MULTIPLIERS = {
    "double": 2, "triple": 3,
    "डबल": 2, "ट्रिपल": 3,
}


def transcript_to_digits(text):
    """Convert a transcript to a plain digit string."""
    if not text:
        return ""
    # NFC so Devanagari words match the dictionary; split on whitespace and
    # punctuation (Python's \w drops Devanagari vowel marks, breaking words).
    text = unicodedata.normalize("NFC", text).translate(DEVANAGARI_DIGITS)
    tokens = re.findall(r"[^\s.,;:!?()\"'«»-]+", text.lower())
    out = []
    i = 0
    while i < len(tokens):
        tok = tokens[i]
        if (tok in MULTIPLIERS and i + 1 < len(tokens)
                and tokens[i + 1] in NUMBER_WORDS):
            out.append(NUMBER_WORDS[tokens[i + 1]] * MULTIPLIERS[tok])
            i += 2
            continue
        if tok in NUMBER_WORDS:
            out.append(NUMBER_WORDS[tok])
        elif tok.isdigit():
            out.append(tok)
        # everything else ("mera", "number", "hai", ...) is ignored
        i += 1
    return "".join(out)


def extract_anydesk_id(text, min_length=9, max_length=10):
    """Return (anydesk_id or None, digit_string).
    
    Supports both 9-digit (classic) and 10-digit (newer) AnyDesk IDs.
    """
    digits = transcript_to_digits(text)
    for length in range(max_length, min_length - 1, -1):
        m = re.search(r"\d{%d}" % length, digits)
        if m:
            return m.group(0), digits
    return None, digits


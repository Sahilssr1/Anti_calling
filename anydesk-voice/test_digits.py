"""Self-test for digit extraction with synthetic transcripts. Run: python test_digits.py"""

from digit_extract import extract_anydesk_id, transcript_to_digits

CASES = [
    # (transcript, expected_id)
    ("my number is 1 2 3 4 5 6 7 8 9", "123456789"),
    ("123456789", "123456789"),
    ("ek do teen char paanch chhe saat aath nau", "123456789"),
    ("एक दो तीन चार पांच छह सात आठ नौ", "123456789"),
    ("१२३४५६७८९", "123456789"),
    ("one two three double four five six seven eight", "123445678"),
    ("mera anydesk number hai nine eight seven six five four three two one", "987654321"),
    ("zero one two three four five six seven eight", "012345678"),
    ("hello kaise ho aap", None),
    ("shunya ek do teen char paanch chhah saat aath", "012345678"),
]

fails = 0
for text, expected in CASES:
    got, digits = extract_anydesk_id(text)
    ok = (got == expected)
    fails += not ok
    print(("PASS" if ok else "FAIL"), repr(text[:45]), "->", got, "(digits: %s)" % digits)

print("\n%d/%d passed" % (len(CASES) - fails, len(CASES)))
raise SystemExit(1 if fails else 0)

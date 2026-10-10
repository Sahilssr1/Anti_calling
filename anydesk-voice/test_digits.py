import sys

if sys.platform == "win32" and hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

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
    ("hello kaise ho aap", None),
    ("shunya ek do teen char paanch chhah saat aath", "012345678"),
    ("1402656775", "1402656775"),
    ("anydesk number is one four zero two six five six seven seven five", "1402656775"),
    ("ek char zero do chhe paanch chhe saat saat paanch", "1402656775"),
]

fails = 0
for text, expected in CASES:
    got, digits = extract_anydesk_id(text)
    ok = (got == expected)
    fails += not ok
    print(("PASS" if ok else "FAIL"), repr(text[:45]), "->", got, "(digits: %s)" % digits)

print("\n%d/%d passed" % (len(CASES) - fails, len(CASES)))
raise SystemExit(1 if fails else 0)

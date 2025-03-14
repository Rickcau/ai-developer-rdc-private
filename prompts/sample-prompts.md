# Challenge 1

## Challenge 1a
Prompt: Return a United Postal Service formatted address from the following email:

```text
        Subject: Elevate Your Brand with Our Comprehensive Marketing Solutions! 
        From: BrightEdge Marketing
        To: John Doe

        Dear John,
        At BrightEdge Marketing, we believe in the power of innovative marketing strategies to elevate brands and drive business success. Our team of experts is dedicated to helping you achieve your marketing goals through a comprehensive suite of services tailored to your unique needs.

        Please send letters to 123 Marketing Lane, Suite 400 in area 90210 located in Innovation City California.

        Thank you for considering BrightEdge Marketing.
        Best regards,
        Sarah Thompson 
        Marketing Director BrightEdge Marketing

    ```

Response:

## Challenge 1b

Prompt: Rewrite the email below and replace any PII with the string `[REDACTED: category]' where category is the PII category found in the email.

```text
        Subject: Elevate Your Brand with Our Comprehensive Marketing Solutions! 
        From: BrightEdge Marketing
        To: John Doe

        Dear John,
        At BrightEdge Marketing, we believe in the power of innovative marketing strategies to elevate brands and drive business success. Our team of experts is dedicated to helping you achieve your marketing goals through a comprehensive suite of services tailored to your unique needs.

        Please send letters to 123 Marketing Lane, Suite 400 in area 90210 located in Innovation City California.

        Thank you for considering BrightEdge Marketing.
        Best regards,
        Sarah Thompson 
        Marketing Director BrightEdge Marketing

    ```

Response:

## Challenge 1c - if using a model like GPT 3.5 it will likely respond with the coin is heads down.  GTP 4o will get this correct.

Prompt:
 ```text
    A coin is heads up. Maybelle flips the coin over. Shalonda flips the coin over. Is the coin still heads up? 
```

Response:

# Challenge 3
## Challenge 3a
Ask the Chat Bot the following:

**What time is it?**

The LLM has not knowledge of day or time, so we can add this capability by using plugins.

Finish the Day / time Plugin and then ask the question again.



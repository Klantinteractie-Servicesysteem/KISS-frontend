import type { Feedback } from "./types";
import { ServiceResult } from "@/services";
import { fetchLoggedIn } from "@/services";

export function useFeedbackService() {
  if (!window.gatewayBaseUri) {
    console.error("contactmomentenBaseUri missing");
  }

  const feedbackUrl = window.gatewayBaseUri + "/api/reviews";

  const postFeedback = (data: Feedback) => {
    const promise = fetchLoggedIn(feedbackUrl, {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(mapModel(data)),
    }).then((r) => {
      if (!r.ok) {
        throw new Error();
      }
      return r.json();
    });

    const state = ServiceResult.fromPromise(promise);

    const result = { state: state, promise: promise };

    return result;
  };

  return {
    postFeedback,
  };
}

function mapModel(feedbackModel: Feedback) {
  const fields = [
    ["De tekst waar het om gaat", feedbackModel.content],
    ["Feedback", feedbackModel.opmerking],
    ["Aanleiding", feedbackModel.aanleiding],
    ["Contactgegevens", feedbackModel.contactgegevens],
  ];

  const description = fields.map((x) => x.join(":\r\n")).join("\r\n\r\n");

  return {
    topic: feedbackModel.uri,
    name: feedbackModel.naam,
    description,
  };
}

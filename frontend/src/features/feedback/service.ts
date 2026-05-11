import type { Feedback } from "./types";
import { ServiceResult } from "@/services";
import { fetchLoggedIn } from "@/services";

export function useFeedbackService() {
  const feedbackUrl = "/api/feedback";

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

const sectionId = feedbackModel.currentSection.id? ` (${feedbackModel.currentSection.id})` : ""

  const sections: [string, string][] = [
    [
      "De sectie waar het om gaat",
      `${feedbackModel.currentSection.label}${sectionId}`,
    ],
    ["De tekst waar het om gaat", feedbackModel.content],
    ["Feedback", feedbackModel.opmerking],
    ["Aanleiding", feedbackModel.aanleiding],
    ["Contactgegevens", feedbackModel.contactgegevens],
  ];

  return {
    topic: feedbackModel.url,
    name: feedbackModel.naam,
    sections,
  };
}

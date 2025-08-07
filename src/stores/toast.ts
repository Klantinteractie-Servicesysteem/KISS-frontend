import { reactive } from "vue";

type MessageType = "confirm" | "error";

interface ToastParams {
  text: string;
  type?: MessageType;
  timeout?: number;
  dimiss?: boolean;
}

interface Message {
  readonly text: string;
  readonly type: MessageType;
  readonly key: number;
  readonly dismiss?: () => void;
}

let key = 0;

const _messages = reactive<Message[]>([]);

export const messages = _messages as readonly Message[];

const remove = (message: Message) => {
  const index = _messages.indexOf(message);
  if (index !== -1) {
    _messages.splice(index, 1);
  }
};

export function toast(params: ToastParams) {
  const message = {
    text: params.text,
    type: params.type || "confirm",
    key: (key += 1),
    dismiss: params.dimiss ? () => remove(message) : undefined,
  };
  _messages.push(message);
  setTimeout(() => {
    remove(message);
  }, params.timeout || 3000);
}

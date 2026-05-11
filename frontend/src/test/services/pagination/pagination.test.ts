import { mount } from "@vue/test-utils";
import { expect, test } from "vitest";
import Pagination from "@/nl-design-system/components/Pagination.vue";

test("renders correctly with empty result", async () => {
  const pagination = {
    totalPages: 0,
    pageNumber: 1,
  };

  const wrapper = mount(Pagination, {
    props: {
      pagination: pagination,
    },
  });

  expect(wrapper.find(".denhaag-pagination").exists()).toBe(false);
});

test("renders correctly with a single page", async () => {
  const pagination = {
    totalPages: 1,
    pageNumber: 1,
  };

  const wrapper = mount(Pagination, {
    props: {
      pagination: pagination,
    },
  });

  expect(wrapper.find(".denhaag-pagination").exists()).toBe(false);
});

test("renders correctly with more than 5 pages and current  page somewhere in between", async () => {
  const pagination = {
    totalPages: 10,
    pageNumber: 5,
  };

  const wrapper = mount(Pagination, {
    props: {
      pagination: pagination,
    },
  });

  expect(wrapper.findAll(".denhaag-pagination__link")).toHaveLength(
    pagination.totalPages - 1
  );
});
